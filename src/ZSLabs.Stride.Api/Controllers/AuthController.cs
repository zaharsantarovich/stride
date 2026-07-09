using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZSLabs.Stride.Api.Contracts;
using ZSLabs.Stride.App.Services;

namespace ZSLabs.Stride.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private const string InvalidCredentialsMessage = "Invalid username or password.";
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<CurrentUser>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUser>> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _authService.AuthenticateAsync(request.Username, request.Password, cancellationToken);
        if (user is null)
        {
            return Unauthorized(new ErrorResponse(InvalidCredentialsMessage));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            },
            CookieAuthenticationDefaults.AuthenticationScheme));

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Ok(new CurrentUser(user.Id, user.Username, user.Email, user.Role));
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<CurrentUser>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUser>> MeAsync(CancellationToken cancellationToken)
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(subject, out var userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetCurrentUserAsync(userId, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new CurrentUser(user.Id, user.Username, user.Email, user.Role));
    }
}