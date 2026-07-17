using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ZSLabs.Stride.Api.Contracts;
using ZSLabs.Stride.Api.Controllers;
using ZSLabs.Stride.App.Services;
using ZSLabs.Stride.Domain.Enums;
using User = ZSLabs.Stride.Domain.Entities.User;

namespace ZSLabs.Stride.Api.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async global::System.Threading.Tasks.Task LoginAsync_ValidCredentials_ReturnsCurrentUserAndSignsIn()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var authService = Substitute.For<IAuthService>();
        var authenticationService = Substitute.For<IAuthenticationService>();
        var user = new User("admin", "hash", "admin@example.com", UserRole.Admin, DateTime.UtcNow) { Id = 42 };
        authService.AuthenticateAsync("admin", "Password123!", cancellationToken).Returns(user);

        var controller = CreateController(authService, authenticationService);

        var result = await controller.LoginAsync(new LoginRequest("admin", "Password123!"), cancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<CurrentUser>(okResult.Value);
        Assert.Equal(42, payload.Id);
        await authenticationService.Received(1).SignInAsync(
            controller.HttpContext,
            "Cookies",
            Arg.Any<ClaimsPrincipal>(),
            Arg.Any<AuthenticationProperties?>());
    }

    [Fact]
    public async global::System.Threading.Tasks.Task LoginAsync_InvalidCredentials_ReturnsUnauthorized()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var authService = Substitute.For<IAuthService>();
        var authenticationService = Substitute.For<IAuthenticationService>();
        authService.AuthenticateAsync("admin", "Password123!", cancellationToken).Returns((User?)null);

        var controller = CreateController(authService, authenticationService);

        var result = await controller.LoginAsync(new LoginRequest("admin", "Password123!"), cancellationToken);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var payload = Assert.IsType<ErrorResponse>(unauthorized.Value);
        Assert.Equal("Invalid username or password.", payload.Message);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task LogoutAsync_AuthenticatedUser_SignsOut()
    {
        var authService = Substitute.For<IAuthService>();
        var authenticationService = Substitute.For<IAuthenticationService>();
        var controller = CreateController(authService, authenticationService, CreatePrincipal(5));

        var result = await controller.LogoutAsync();

        Assert.IsType<NoContentResult>(result);
        await authenticationService.Received(1).SignOutAsync(
            controller.HttpContext,
            "Cookies",
            Arg.Any<AuthenticationProperties?>());
    }

    [Fact]
    public async global::System.Threading.Tasks.Task MeAsync_Unauthenticated_ReturnsUnauthorized()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var authService = Substitute.For<IAuthService>();
        var authenticationService = Substitute.For<IAuthenticationService>();
        var controller = CreateController(authService, authenticationService);

        var result = await controller.MeAsync(cancellationToken);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task MeAsync_Authenticated_ReturnsCurrentUser()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var authService = Substitute.For<IAuthService>();
        var authenticationService = Substitute.For<IAuthenticationService>();
        var user = new User("jane", "hash", "jane@example.com", UserRole.Regular, DateTime.UtcNow) { Id = 8 };
        authService.GetCurrentUserAsync(8, cancellationToken).Returns(user);
        var controller = CreateController(authService, authenticationService, CreatePrincipal(8));

        var result = await controller.MeAsync(cancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<CurrentUser>(okResult.Value);
        Assert.Equal("jane", payload.Username);
    }

    private static AuthController CreateController(
        IAuthService authService,
        IAuthenticationService authenticationService,
        ClaimsPrincipal? principal = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(authenticationService);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider(),
            User = principal ?? new ClaimsPrincipal(new ClaimsIdentity()),
        };

        return new AuthController(authService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            },
        };
    }

    private static ClaimsPrincipal CreatePrincipal(int userId)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
            "Cookies"));
    }
}