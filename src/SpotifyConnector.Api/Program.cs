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


//********************APPLICATION*********************
var app = builder.Build();

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

    app.Run();

