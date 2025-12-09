using SpotifyConnector.Spotify;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// *******************BUILDER*************************

// Bind Spotify options from configuration
builder.Services.Configure<SpotifyOptions>(
    builder.Configuration.GetSection(SpotifyOptions.SectionName));

// Register a typed HttpClient for ISpotifyAuthService
builder.Services.AddHttpClient<ISpotifyAuthService, SpotifyAuthService>();

// In-memory token store
builder.Services.AddSingleton<ISpotifyTokenStore, MemorySpotifyTokenStore>();

// Api client for interactions i.e. adding playlists
builder.Services.AddHttpClient<ISpotifyApiClient, SpotifyApiClient>();


//********************APPLICATION*********************
var app = builder.Build();

//*********************END POINTS*********************

// Basic health enpoint
    app.MapGet("/health", () => Results.Ok(new {status = "ok"}));

// Start Spotify auth flow
    app.MapGet("/auth/login", (ISpotifyAuthService authService) => {
        // simulation of real app random state generation and storage (cookies / cache).
        var state = Guid.NewGuid().ToString("N");
        var authUrl = authService.BuildAuthorizeUrl(state);

        // for dev just redirect straight there.
        return Results.Redirect(authUrl); 
    });

    // Callback endpoint from Spotify (TODO: finish later)
    app.MapGet("/auth/callback", async(
        string code, 
        string state, 
        ISpotifyAuthService authService,
        ISpotifyTokenStore tokenStore,
        CancellationToken ct) => {

            // TODO: validate state in the real app
            
            var token = await authService.ExchangeCodeForTokenAsync(code, ct);

            await tokenStore.StoreTokenAsync(token, ct);

            return Results.Ok(new {
                message = "Token exchange succeeded and token stored.",
                token.ExpiresIn,
                token.Scope,
                token.TokenType
            });
        });

// User information endpoint
    app.MapGet("/me", async (
        ISpotifyTokenStore tokenStore,
        IHttpClientFactory httpClientFactory,
        CancellationToken ct) => {
            var token = await tokenStore.GetTokenAsync(ct);

            if(token is null || string.IsNullOrWhiteSpace(token.AccessToken)) {
                return Results.Problem(
                    "No accss token stored.  Call /auth/login first to authenticate with Spotify.",
                    statusCode: 401);
            }

            var client = httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token.AccessToken);

            using var response = await client.GetAsync("https://api.spotify.com/v1/me", ct);
            var content = await response.Content.ReadAsStringAsync(ct);

            if(!response.IsSuccessStatusCode) {
                // TODO: log content later.  Just bubbling the status code for now
                return Results.StatusCode((int)response.StatusCode); 
            }

            return Results.Content(content, "application/json");
        }
    );

// Playlist import endpoint
    app.MapPost("/playlists/import", async (
    PlaylistImportRequest request,
    ISpotifyApiClient spotifyApiClient,
    CancellationToken ct) => {
           if(string.IsNullOrWhiteSpace(request.Name)) {
                return Results.BadRequest("Playlist name is required");
            }

            if(request.Tracks is null || request.Tracks.Count == 0) {
                return Results.BadRequest("At least one track is required");
            }

            // 1. Get current user id
            var userId = await spotifyApiClient.GetCurrentUserIdAsync(ct);

            // 2. Create the playlist
            var playlistId = await spotifyApiClient.CreatePlaylistAsync(
                userId,
                request.Name,
                request.Description,
                request.Public,
                ct);

            // 3. Search for each track and collect URIs
            var urls = new List<string>();

            foreach(var track in request.Tracks) {
                var url = await spotifyApiClient.FindTrackUriAsync(track.Search, ct);

                if(!string.IsNullOrWhiteSpace(url)) {
                    urls.Add(url);
                }
            } 

            // 4. Add tracks to playlist
            await spotifyApiClient.AddTracksToPlaylistAsync(playlistId, urls, ct);

            return Results.Ok(new {
                message = "Playlist created from import.",
                playlistId,
                totalTracksRequested = request.Tracks.Count,
                totalTracksAdded = urls.Count
            });  
    });

    app.Run();

