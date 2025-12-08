namespace SpotifyConnector.Spotify;

public class SpotifyOptions {
    public const string SectionName = "Spotify";

    public string ClientId {get;set;} = string.Empty;
    public string ClientSecret {get;set;} = string.Empty;
    public string RedirectUri {get;set;} = string.Empty;
    public string[] Scopes {get;set;} = [
        "playlist-modify-public",
        "playlist-modify-private",
        "user-read-email",
        "user-read-playback-state",
        "user-modify-playback-state"
    ];
}