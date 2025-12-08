using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace SpotifyConnector.Spotify;

public class SpotifyAuthService : ISpotifyAuthService
{
    private readonly SpotifyOptions _options;
    private readonly HttpClient _httpClient;

    private const string AuthorizeEndpoint = "https://accounts.spotify.com/authorize";
    private const string TokenEndpoint = "https://accounts.spotify.com/api/token";

    public SpotifyAuthService(IOptions<SpotifyOptions> options, HttpClient httpClient)
    {
        _options = options.Value;
        _httpClient = httpClient;
    }

    public string BuildAuthorizeUrl(string state)
{
    var scopes = string.Join(' ', _options.Scopes);

    var query = new StringBuilder();
    query.Append($"client_id={Uri.EscapeDataString(_options.ClientId)}");
    query.Append("&response_type=code");
    query.Append($"&redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}");
    query.Append($"&scope={Uri.EscapeDataString(scopes)}");
    query.Append($"&state={Uri.EscapeDataString(state)}");

    var url = $"{AuthorizeEndpoint}?{query}";
    Console.WriteLine($"Authorize URL: {url}");
    return url;
}


    public async Task<SpotifyTokenResponse> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken = default)
    {
        // Basic Auth header:  base64(clientId:clientSecret)
        var basicAuthBytes = Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}");
        var basicAuth = Convert.ToBase64String(basicAuthBytes);

        using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint);

        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);

        request.Content = new FormUrlEncodedContent(new[] {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", _options.RedirectUri),
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if(!response.IsSuccessStatusCode) {
            // For now: throw with body so I can see what Spotify said
            throw new InvalidOperationException(
                $"Spotify token request failed ({(int)response.StatusCode}): {body}"
            );
        }

        var token = JsonSerializer.Deserialize<SpotifyTokenResponse>(
            body,
            new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            });

        if (token is null){
            throw new InvalidOperationException("Failed to deseralize Spotify token response.");
        }

        return token;
    }
}
