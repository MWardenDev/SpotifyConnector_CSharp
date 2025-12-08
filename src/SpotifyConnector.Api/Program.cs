using SpotifyConnector.Spotify;

var builder = WebApplication.CreateBuilder(args);

// Bind Spotify options from configuration
builder.Services.Configure<SpotifyOptions>(
    builder.Configuration.GetSection(SpotifyOptions.SectionName));

// Register a typed HttpClient for ISpotifyAuthService
builder.Services.AddHttpClient<ISpotifyAuthService, SpotifyAuthService>();

var app=builder.Build();

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
    CancellationToken ct) => {

        // TODO: validate state against waht we stored.
        
        var token = await authService.ExchangeCodeForTokenAsync(code, ct);

        // For now just return the token payload so we can see it work
        return Results.Ok(new {
        message = "Token exchange succeeded.",
        token.AccessToken,
        token.RefreshToken,
        token.ExpiresIn,
        token.Scope,
        token.TokenType
        });
    });

    app.Run();

