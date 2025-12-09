namespace SpotifyConnector.Spotify;

public interface ISpotifyTokenStore {
    Task StoreTokenAsync(SpotifyTokenResponse token, CancellationToken cancellationToken=default);

    Task<SpotifyTokenResponse?> GetTokenAsync(CancellationToken cancellationToken = default);
}