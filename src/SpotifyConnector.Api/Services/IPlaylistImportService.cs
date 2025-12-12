using SpotifyConnector.Api.Models;

namespace SpotifyConnector.Api.Services;

public interface IPlaylistImportService
{
    Task<PlaylistImportResult> ImportAsync(PlaylistImportRequest request, CancellationToken ct);
}
