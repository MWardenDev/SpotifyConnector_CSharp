using Microsoft.AspNetCore.Mvc;
using SpotifyConnector.Api.Models;
using SpotifyConnector.Api.Services;

namespace SpotifyConnector.Api.Controllers;

[ApiController]
[Route("playlists")]
public class PlaylistsController : ControllerBase
{
    private readonly IPlaylistImportService _playlistImportService;

    public PlaylistsController(IPlaylistImportService playlistImportService)
    {
        _playlistImportService = playlistImportService;
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromBody] PlaylistImportRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Playlist name is required");

        if (request.Tracks is null || request.Tracks.Count == 0)
            return BadRequest("At least one track is required");

        var result = await _playlistImportService.ImportAsync(request, ct);
        return Ok(result);
    }
}
