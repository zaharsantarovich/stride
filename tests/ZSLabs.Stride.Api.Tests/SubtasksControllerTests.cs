using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ZSLabs.Stride.Api.Contracts;
using ZSLabs.Stride.Api.Controllers;
using ZSLabs.Stride.App.Services;
using DomainSubtask = ZSLabs.Stride.Domain.Entities.Subtask;
using DomainSubtaskStatus = ZSLabs.Stride.Domain.Enums.SubtaskStatus;

namespace ZSLabs.Stride.Api.Tests;

public class SubtasksControllerTests
{
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
        service.CreateSubtaskAsync(4, 8, Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<DomainSubtaskStatus?>(), Arg.Any<int?>(), Arg.Any<DateTime?>(), cancellationToken)
            .Returns(new DomainSubtask(4, "Subtask", null, DomainSubtaskStatus.Todo, 8, null, null, DateTime.UtcNow));

        var controller = CreateController(service, 8);
        var result = await controller.CreateAsync(4, new CreateSubtaskRequest("Subtask", null, null, null, null), cancellationToken);

        Assert.IsType<CreatedResult>(result.Result);
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