namespace SpotifyConnector.Spotify;

public interface ISpotifyAuthService {
    /// <summary>
    /// Builds the Spotify authorization URL to redirect the user to.
    /// </summary>
    string BuildAuthorizeUrl(string state);

    // Impliment in next phase
    Task<SpotifyTokenResponse> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken = default);

}