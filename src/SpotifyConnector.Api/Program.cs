using SpotifyConnector.Api.Services;
using SpotifyConnector.Spotify;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// *******************SERVICES*************************

// Bind Spotify options from configuration
builder.Services.Configure<SpotifyOptions>(
    builder.Configuration.GetSection(SpotifyOptions.SectionName));

// Register a typed HttpClient for ISpotifyAuthService
builder.Services.AddHttpClient<ISpotifyAuthService, SpotifyAuthService>();

// In-memory token store
builder.Services.AddSingleton<ISpotifyTokenStore, MemorySpotifyTokenStore>();

// Api client for interactions i.e. adding playlists
builder.Services.AddHttpClient<ISpotifyApiClient, SpotifyApiClient>();
builder.Services.AddScoped<IPlaylistImportService, PlaylistImportService>();

builder.Services.AddControllers();


//********************APPLICATION*********************
var app = builder.Build();

app.MapControllers();

app.Run();

