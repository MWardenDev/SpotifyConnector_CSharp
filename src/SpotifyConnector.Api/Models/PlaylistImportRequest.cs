namespace SpotifyConnector.Api.Models;

public class PlaylistImportRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public bool Public { get; set; } = false;

    // Each track is defined by a search string you’ll feed to Spotify’s search API
    public List<PlaylistTrackDefinition> Tracks { get; set; } = new();
}

public class PlaylistTrackDefinition
{
    // e.g. "Anvil of Crom Conan the Barbarian"
    public string Search { get; set; } = default!;
}
