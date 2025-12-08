using System.Text.Json.Serialization;

namespace SpotifyConnector.Spotify;

public class SpotifyTokenResponse {
    [JsonPropertyName("access_token")]
    public string AccessToken {get;set;} = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType {get;set;} = "Bearer";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn {get;set;}            // seconds

    [JsonPropertyName("refresh_token")]
    public string RefreshToken {get;set;} = string.Empty;

    [JsonPropertyName("scope")]
    public string Scope {get;set;} = string.Empty;
}