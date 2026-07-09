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
    public void ControllerRequiresAdminOnlyPolicy()
    {
        var attribute = typeof(UsersController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("AdminOnly", attribute.Policy);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task CreateAsyncReturnsConflictForDuplicateUsername()
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
    public async global::System.Threading.Tasks.Task UpdateAsyncReturnsNotFoundForMissingUser()
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