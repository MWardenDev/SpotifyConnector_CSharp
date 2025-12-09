using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace SpotifyConnector.Spotify;

public class SpotifyApiClient : ISpotifyApiClient {
    private readonly HttpClient _httpClient;
    private readonly ISpotifyTokenStore _tokenStore;

    private const string ApiBaseUrl = "https://api.spotify.com/v1";

    public SpotifyApiClient(HttpClient httpClient, ISpotifyTokenStore tokenStore) {
        _httpClient = httpClient;
        _tokenStore = tokenStore;
    }

    private async Task<HttpClient> GetAuthorizedClientAsync(CancellationToken cancellationToken) {
        var token = await _tokenStore.GetTokenAsync(cancellationToken);

        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken)) {
            throw new InvalidOperationException("No access token available.  Call /auth/login first");
        }

        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token.AccessToken);

        return _httpClient;
    }

    public async Task<string> GetCurrentUserIdAsync(CancellationToken cancellationToken = default) {
        var client = await GetAuthorizedClientAsync(cancellationToken);

        using var response = await client.GetAsync($"{ApiBaseUrl}/me", cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if(!response.IsSuccessStatusCode) {
            throw new InvalidOperationException(
                $"Failed to fetch current user. Status {(int)response.StatusCode}: {json}");
        }

        using var doc = JsonDocument.Parse(json);
        var id = doc.RootElement.GetProperty("id").GetString();

        if (string.IsNullOrWhiteSpace(id)) {
            throw new InvalidOperationException("Spotify user id not found in /me response.");
        }

        return id;
    }

    public async Task<string> CreatePlaylistAsync(
        string userId,
        string name,
        string? description,
        bool isPublic,
        CancellationToken cancellationToken = default) {
        var client = await GetAuthorizedClientAsync(cancellationToken);

        var payload = new {
            name,
            description,
            @public = isPublic
        };

        var jsonBody = JsonSerializer.Serialize(payload);
        using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        using var response =
            await client.PostAsync($"{ApiBaseUrl}/users/{Uri.EscapeDataString(userId)}/playlists", content, cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if(!response.IsSuccessStatusCode) {
            throw new InvalidOperationException(
                $"Failed to create playlist.  Status {(int)response.StatusCode}: {json}" );
        }

        using var doc = JsonDocument.Parse(json);
        var playlistId = doc.RootElement.GetProperty("id").GetString();

        if (string.IsNullOrWhiteSpace(playlistId)) {
            throw new InvalidOperationException("Playlist id not found in create playlist response.");
        }

        return playlistId;
    }

    public async Task<string?> FindTrackUriAsync(
        string searchQuery,
        CancellationToken cancellationToken = default) {
        if(string.IsNullOrWhiteSpace(searchQuery)) {
            return null;
        }

        var client = await GetAuthorizedClientAsync(cancellationToken);

        var query = Uri.EscapeDataString(searchQuery);
        var url = $"{ApiBaseUrl}/search?q={query}&type=track&limit=1";

        using var response = await client.GetAsync(url, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if(!response.IsSuccessStatusCode) {
            // for now , we will skip tracks that fail search
            return null;
        }

        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("tracks", out var tracksElement)) {
            return null;
        }

        if (!tracksElement.TryGetProperty("items", out var itemsElement)) {
            return null;
        }

        if(itemsElement.GetArrayLength() == 0) {
            return null;
        }

        var first = itemsElement[0];

        if (!first.TryGetProperty("uri", out var uriElement)) {
            return null;
        }

        var uri = uriElement.GetString();
        return string.IsNullOrWhiteSpace(uri) ? null : uri;
    }
}