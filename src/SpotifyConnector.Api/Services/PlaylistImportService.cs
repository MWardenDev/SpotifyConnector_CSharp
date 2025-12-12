using SpotifyConnector.Api.Models;
using SpotifyConnector.Spotify;

namespace SpotifyConnector.Api.Services;

public sealed class PlaylistImportService : IPlaylistImportService {
    private readonly ISpotifyApiClient _spotifyApiClient;

    public PlaylistImportService(ISpotifyApiClient spotifyApiClient) {
        _spotifyApiClient = spotifyApiClient;
    }

    public async Task<PlaylistImportResult> ImportAsync(PlaylistImportRequest request, CancellationToken ct) {
        // Business logic only. No Http concerns here.
        
        // 1. Get user id
        var userId = await _spotifyApiClient.GetCurrentUserIdAsync(ct);

        // 2. Create playlist
        var playlistId = await _spotifyApiClient.CreatePlaylistAsync(
            userId,
            request.Name,
            request.Description,
            request.Public,
            ct);

        // 3. Search for each track and collect URIs
        var uris = new List<string>();
        
        foreach(var track in request.Tracks) {
            var uri = await _spotifyApiClient.FindTrackUriAsync(track.Search, ct);

            if(!string.IsNullOrWhiteSpace(uri))
                uris.Add(uri);
        }

        // 4. Add tracks to playlist
        await _spotifyApiClient.AddTracksToPlaylistAsync(playlistId, uris, ct);

        // 5. return summary
        return new PlaylistImportResult {
            PlaylistId = playlistId,
            TotalTracksRequested = request.Tracks.Count,
            TotalTracksAdded = uris.Count
        };
    }
}