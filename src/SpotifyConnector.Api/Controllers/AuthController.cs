using Microsoft.AspNetCore.Mvc;
using SpotifyConnector.Spotify;

namespace SpotifyConnector.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase {
    private readonly ISpotifyAuthService _authService;
    private readonly ISpotifyTokenStore _tokenStore;

    public AuthController(
        ISpotifyAuthService authService,
        ISpotifyTokenStore tokenStore) {
        _authService = authService;
        _tokenStore = tokenStore;
    }

    [HttpGet("login")]
    public IActionResult Login() {
        // TODO: store state properly later
        var state = Guid.NewGuid().ToString("N");
        var authUrl = _authService.BuildAuthorizeUrl(state);

        return Redirect(authUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string code,
        [FromQuery] string state,
        CancellationToken ct) {
        
        // TODO: validate state in real app
        var token = await _authService.ExchangeCodeForTokenAsync(code, ct);
        await _tokenStore.StoreTokenAsync(token, ct);

        return Ok(new {
            message = "Token exchange succeeded and tokan stored.",
            token.ExpiresIn,
            token.Scope, 
            token.TokenType
        });
    }
}