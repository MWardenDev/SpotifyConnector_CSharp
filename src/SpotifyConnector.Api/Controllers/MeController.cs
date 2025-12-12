using Microsoft.AspNetCore.Mvc;
using SpotifyConnector.Spotify;
using System.Net.Http.Headers;

namespace SpotifyConnector.Api.Controllers;

[ApiController]
[Route("me")]
public class MeControler : ControllerBase {
    private readonly ISpotifyTokenStore _tokenStore;
    private readonly IHttpClientFactory _httpClientFactory;

    public MeControler(
        ISpotifyTokenStore tokenStore,
        IHttpClientFactory httpClientFactory) {
        _tokenStore = tokenStore;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) {
        var token = await _tokenStore.GetTokenAsync(ct);

        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken)) {
            return Problem(
                "No access token stored. Call /auth/login first",
                statusCode: 401);
        }

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token.AccessToken);

        using var response = await client.GetAsync("https://api.spotify.com/v1/me", ct);
        var content = await response.Content.ReadAsStringAsync(ct);

        if(!response.IsSuccessStatusCode) {
            return StatusCode((int)response.StatusCode);
        }

        return Content(content, "application/json");
    }
}