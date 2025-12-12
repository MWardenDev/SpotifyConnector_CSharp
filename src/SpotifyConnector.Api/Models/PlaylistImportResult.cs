namespace SpotifyConnector.Api.Models;

public sealed class PlaylistImportResult {
    public string Message { get; init; } = "Playlist created from import.";
    public string PlaylistId { get; init; } = "";
    public int TotalTracksRequested { get; init;}
    public int TotalTracksAdded { get; init; }
}