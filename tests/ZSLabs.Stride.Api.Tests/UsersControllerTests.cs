using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ZSLabs.Stride.Api.Contracts;
using ZSLabs.Stride.Api.Controllers;
using ZSLabs.Stride.App.Services;
using ZSLabs.Stride.Domain.Entities;
using ZSLabs.Stride.Domain.Enums;
using DomainUser = ZSLabs.Stride.Domain.Entities.User;

namespace ZSLabs.Stride.Api.Tests;

public class UsersControllerTests
{
    [Fact]
    public void UserManagementActions_MultipleEndpoints_RequireAdminOnlyPolicy()
    {
        var getAttribute = typeof(UsersController)
            .GetMethod(nameof(UsersController.GetAsync))!
            .GetCustomAttribute<AuthorizeAttribute>();
        var createAttribute = typeof(UsersController)
            .GetMethod(nameof(UsersController.CreateAsync))!
            .GetCustomAttribute<AuthorizeAttribute>();
        var updateAttribute = typeof(UsersController)
            .GetMethod(nameof(UsersController.UpdateAsync))!
            .GetCustomAttribute<AuthorizeAttribute>();

        Assert.Equal("AdminOnly", getAttribute?.Policy);
        Assert.Equal("AdminOnly", createAttribute?.Policy);
        Assert.Equal("AdminOnly", updateAttribute?.Policy);
    }

    [Fact]
    public void GetRegularUsersAsync_AuthorizationPolicy_RequiresRegularOnly()
    {
        var attribute = typeof(UsersController)
            .GetMethod(nameof(UsersController.GetRegularUsersAsync))!
            .GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("RegularOnly", attribute.Policy);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task GetRegularUsersAsync_NoFilter_ReturnsLookupContracts()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userService = Substitute.For<IUserService>();
        userService.GetRegularUsersAsync(cancellationToken).Returns([
            new DomainUser("anna", "hash", null, UserRole.Regular, DateTime.UtcNow) { Id = 3 },
            new DomainUser("zoe", "hash", null, UserRole.Regular, DateTime.UtcNow) { Id = 5 }
        ]);

        var controller = new UsersController(userService);

        var result = await controller.GetRegularUsersAsync(cancellationToken);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsAssignableFrom<IReadOnlyList<RegularUserLookup>>(ok.Value);
        Assert.Collection(
            payload,
            user => Assert.Equal(new RegularUserLookup(3, "anna"), user),
            user => Assert.Equal(new RegularUserLookup(5, "zoe"), user));
    }

    [Fact]
    public async global::System.Threading.Tasks.Task CreateAsync_DuplicateUsername_ReturnsConflict()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userService = Substitute.For<IUserService>();
        userService
            .CreateRegularUserAsync("taken", "Password123!", null, cancellationToken)
            .Returns(_ => global::System.Threading.Tasks.Task.FromException<DomainUser>(new InvalidOperationException("Username already exists.")));

        var controller = new UsersController(userService);

        var result = await controller.CreateAsync(new CreateUserRequest("taken", "Password123!", null), cancellationToken);

        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        var payload = Assert.IsType<ErrorResponse>(conflict.Value);
        Assert.Equal("Username already exists.", payload.Message);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task UpdateAsync_MissingUser_ReturnsNotFound()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userService = Substitute.For<IUserService>();
        userService
            .UpdateRegularUserAsync(7, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), cancellationToken)
            .Returns(_ => global::System.Threading.Tasks.Task.FromException<DomainUser>(new KeyNotFoundException()));

        var controller = new UsersController(userService);

        var result = await controller.UpdateAsync(7, new UpdateUserRequest("name", null, null), cancellationToken);

        Assert.IsType<NotFoundResult>(result.Result);
    }
}