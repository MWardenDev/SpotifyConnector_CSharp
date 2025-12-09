namespace SpotifyConnector.Spotify;

public interface ISpotifyApiClient {
    Task<string> GetCurrentUserIdAsync(CancellationToken cancellationToken = default);

    Task<string> CreatePlaylistAsync( 
        string userId,
        string name,
        string? description,
        bool isPublic,
        CancellationToken cancellationToken = default
    ) ;

    Task<string?> FindTrackUriAsync (
        string searchQuery,
        CancellationToken cancellationToken = default
    );

    Task AddTracksToPlaylistAsync( 
        string playlistId,
        IReadOnlyCollection<string> trackUris,
        CancellationToken cancellationToken = default
    );
}