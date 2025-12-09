namespace SpotifyConnector.Spotify;

public class PlayListImportRequest {
    public string Name{ get; set; } = string.Empty;
    public string? Description { get; set; }

    // Whether the playlist shoud be public (false = private)
    public bool Public { get; set; } = false;

    public List<PlaylistTrackDefinition> Tracks { get; set; } = new();
}

public class PlaylistTrackDefinition {
    // Free form search text, e.g. "Conan the Barbarian Anvil of Crom"
    public string Search { get; set; } = string.Empty;
}