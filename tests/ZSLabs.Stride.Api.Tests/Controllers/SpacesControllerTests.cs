using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ZSLabs.Stride.Api.Contracts;
using ZSLabs.Stride.Api.Controllers;
using ZSLabs.Stride.App.Services;
using ZSLabs.Stride.Domain.Entities;
using DomainSpace = ZSLabs.Stride.Domain.Entities.Space;

namespace ZSLabs.Stride.Api.Tests.Controllers;

public class SpacesControllerTests
{
    [Fact]
    public void SpacesController_ClassAttribute_RequiresRegularOnlyPolicy()
    {
        var attribute = typeof(SpacesController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("RegularOnly", attribute.Policy);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task GetByIdAsync_PrivateSpaceNoAccess_ReturnsForbidden()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var spaceService = Substitute.For<ISpaceService>();
        spaceService.GetSpaceAsync(5, 8, cancellationToken)
            .Returns(_ => global::System.Threading.Tasks.Task.FromException<DomainSpace>(new UnauthorizedAccessException("No access.")));

        var controller = CreateController(spaceService, 8);

        var result = await controller.GetByIdAsync(5, cancellationToken);

        Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, ((ObjectResult)result.Result!).StatusCode);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task CreateAsync_DuplicateKey_ReturnsConflict()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var spaceService = Substitute.For<ISpaceService>();
        spaceService.CreateSpaceAsync(8, "dup", "Name", true, cancellationToken)
            .Returns(_ => global::System.Threading.Tasks.Task.FromException<DomainSpace>(new InvalidOperationException("Space key already exists.")));

        var controller = CreateController(spaceService, 8);

        var result = await controller.CreateAsync(new CreateSpaceRequest("dup", "Name", true), cancellationToken);

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task UpdateAsync_NonAuthorVisibilityChange_ReturnsForbidden()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var spaceService = Substitute.For<ISpaceService>();
        spaceService.UpdateSpaceAsync(4, 8, "Name", false, cancellationToken)
            .Returns(_ => global::System.Threading.Tasks.Task.FromException<DomainSpace>(new UnauthorizedAccessException("Only the author can change space visibility.")));

        var controller = CreateController(spaceService, 8);

        var result = await controller.UpdateAsync(4, new UpdateSpaceRequest("Name", false), cancellationToken);

        Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, ((ObjectResult)result.Result!).StatusCode);
    }

    private static SpacesController CreateController(ISpaceService spaceService, int userId)
    {
        var controller = new SpacesController(spaceService);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                }, "Cookies")),
            },
        };

        return controller;
    }
}