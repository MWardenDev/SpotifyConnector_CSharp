namespace SpotifyConnector.Spotify;

public class MemorySpotifyTokenStore : ISpotifyTokenStore {
    private SpotifyTokenResponse? _currentToken;
    private readonly object _lock = new();

    public Task StoreTokenAsync(SpotifyTokenResponse token, CancellationToken cancellationToken = default) {
        lock(_lock) {
            _currentToken = token;
        }

        return Task.CompletedTask;
    }

    public Task<SpotifyTokenResponse?> GetTokenAsync(CancellationToken cancellationToken = default) {
        lock (_lock) {
            return Task.FromResult(_currentToken);
        }
    }
}