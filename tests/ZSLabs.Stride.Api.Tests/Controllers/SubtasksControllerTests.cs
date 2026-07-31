using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ZSLabs.Stride.Api.Contracts;
using ZSLabs.Stride.Api.Controllers;
using ZSLabs.Stride.App.Services;
using DomainComment = ZSLabs.Stride.Domain.Entities.Comment;
using DomainSubtask = ZSLabs.Stride.Domain.Entities.Subtask;
using DomainSubtaskStatus = ZSLabs.Stride.Domain.Enums.SubtaskStatus;
using DomainUser = ZSLabs.Stride.Domain.Entities.User;
using UserRole = ZSLabs.Stride.Domain.Enums.UserRole;

namespace ZSLabs.Stride.Api.Tests.Controllers;

public class SubtasksControllerTests
{
    [Fact]
    public async global::System.Threading.Tasks.Task GetAsync_ExistingSubtask_ReturnsCompleteMappedSubtask()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = Substitute.For<ISubtaskService>();
        var author = new DomainUser("author", "hash", null, UserRole.Regular, DateTime.UtcNow) { Id = 8 };
        var commenter = new DomainUser("commenter", "hash", null, UserRole.Regular, DateTime.UtcNow) { Id = 9 };
        var subtask = new DomainSubtask(4, "Subtask", null, DomainSubtaskStatus.Todo, author.Id, null, null, DateTime.UtcNow)
        {
            Id = 6,
            Author = author,
        };
        subtask.Comments.Add(new DomainComment(null, subtask.Id, commenter.Id, "Comment", DateTime.UtcNow)
        {
            Id = 7,
            Author = commenter,
        });
        service.GetSubtaskAsync(6, 8, cancellationToken).Returns(subtask);

        var result = await CreateController(service, 8).GetAsync(6, cancellationToken);

        var response = Assert.IsType<OkObjectResult>(result.Result);
        var contract = Assert.IsType<Subtask>(response.Value);
        Assert.Equal("author", contract.AuthorUsername);
        Assert.Equal("commenter", Assert.Single(contract.Comments).AuthorUsername);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task GetAsync_NoAccess_ReturnsForbidden()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = Substitute.For<ISubtaskService>();
        service.GetSubtaskAsync(6, 8, cancellationToken)
            .Returns(_ => global::System.Threading.Tasks.Task.FromException<DomainSubtask>(new UnauthorizedAccessException("No access.")));

        var result = await CreateController(service, 8).GetAsync(6, cancellationToken);

        var response = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, response.StatusCode);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task GetAsync_MissingSubtask_ReturnsNotFound()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = Substitute.For<ISubtaskService>();
        service.GetSubtaskAsync(6, 8, cancellationToken)
            .Returns(_ => global::System.Threading.Tasks.Task.FromException<DomainSubtask>(new KeyNotFoundException()));

        var result = await CreateController(service, 8).GetAsync(6, cancellationToken);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void SubtasksController_ClassAttribute_RequiresRegularOnlyPolicy()
    {
        var attribute = typeof(SubtasksController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("RegularOnly", attribute.Policy);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task CreateAsync_ValidRequest_ReturnsCreatedSubtask()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = Substitute.For<ISubtaskService>();
        var author = new DomainUser("author", "hash", null, UserRole.Regular, DateTime.UtcNow) { Id = 8 };
        service.CreateSubtaskAsync(4, 8, Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<DomainSubtaskStatus?>(), Arg.Any<int?>(), Arg.Any<DateTime?>(), cancellationToken)
            .Returns(new DomainSubtask(4, "Subtask", null, DomainSubtaskStatus.Todo, 8, null, null, DateTime.UtcNow)
            {
                Author = author,
            });

        var controller = CreateController(service, 8);
        var result = await controller.CreateAsync(4, new CreateSubtaskRequest("Subtask", null, null, null, null), cancellationToken);

        var response = Assert.IsType<CreatedResult>(result.Result);
        var contract = Assert.IsType<Subtask>(response.Value);
        Assert.Equal("author", contract.AuthorUsername);
        Assert.Empty(contract.Comments);
        Assert.NotEqual(default, contract.CreatedAt);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task UpdateAsync_NoAccess_ReturnsForbidden()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = Substitute.For<ISubtaskService>();
        service.UpdateSubtaskAsync(6, 8, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<DomainSubtaskStatus?>(), Arg.Any<int?>(), Arg.Any<DateTime?>(), cancellationToken)
            .Returns(_ => global::System.Threading.Tasks.Task.FromException<DomainSubtask>(new UnauthorizedAccessException("No access.")));

        var controller = CreateController(service, 8);
        var result = await controller.UpdateAsync(6, new UpdateSubtaskRequest("Renamed", null, Contracts.SubtaskStatus.Done, null, null), cancellationToken);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task DeleteAsync_ExistingSubtask_ReturnsNoContent()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = Substitute.For<ISubtaskService>();
        var controller = CreateController(service, 8);

        var result = await controller.DeleteAsync(6, cancellationToken);

        Assert.IsType<NoContentResult>(result);
    }

    private static SubtasksController CreateController(ISubtaskService service, int userId)
    {
        var controller = new SubtasksController(service);
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