using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ZSLabs.Stride.Api.Contracts;
using ZSLabs.Stride.Api.Controllers;
using ZSLabs.Stride.App.Services;
using DomainTask = ZSLabs.Stride.Domain.Entities.Task;
using DomainTaskPriority = ZSLabs.Stride.Domain.Enums.TaskPriority;
using DomainTaskStatus = ZSLabs.Stride.Domain.Enums.TaskStatus;

namespace ZSLabs.Stride.Api.Tests.Controllers;

public class TasksControllerTests
{
    [Fact]
    public void TasksController_ClassAttribute_RequiresRegularOnlyPolicy()
    {
        var attribute = typeof(TasksController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("RegularOnly", attribute.Policy);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task GetAsync_ValidSpaceAndUser_ReturnsTasks()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = Substitute.For<ITaskService>();
        service.GetTasksAsync(3, 8, cancellationToken).Returns([
            new DomainTask(3, "Task", null, DomainTaskStatus.Backlog, DomainTaskPriority.High, 8, null, null, DateTime.UtcNow),
        ]);

        var controller = CreateController(service, 8);
        var result = await controller.GetAsync(3, cancellationToken);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var tasks = Assert.IsAssignableFrom<IReadOnlyList<Contracts.Task>>(ok.Value);
        Assert.Single(tasks);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task CreateAsync_NoSpaceAccess_ReturnsForbidden()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = Substitute.For<ITaskService>();
        service.CreateTaskAsync(3, 8, Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<DomainTaskStatus?>(), Arg.Any<DomainTaskPriority>(), Arg.Any<int?>(), Arg.Any<DateTime?>(), cancellationToken)
            .Returns(_ => global::System.Threading.Tasks.Task.FromException<DomainTask>(new UnauthorizedAccessException("No access.")));

        var controller = CreateController(service, 8);
        var result = await controller.CreateAsync(3, new CreateTaskRequest("Task", null, null, Contracts.TaskPriority.High, null, null), cancellationToken);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task UpdateStatusAsync_ValidStatus_ReturnsUpdatedTask()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = Substitute.For<ITaskService>();
        service.UpdateTaskStatusAsync(11, 8, DomainTaskStatus.Done, cancellationToken)
            .Returns(new DomainTask(3, "Task", null, DomainTaskStatus.Done, DomainTaskPriority.Medium, 8, null, null, DateTime.UtcNow));

        var controller = CreateController(service, 8);
        var result = await controller.UpdateStatusAsync(11, new UpdateTaskStatusRequest(Contracts.TaskStatus.Done), cancellationToken);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var task = Assert.IsType<Contracts.Task>(ok.Value);
        Assert.Equal(Contracts.TaskStatus.Done, task.Status);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task DeleteAsync_ExistingTask_ReturnsNoContent()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = Substitute.For<ITaskService>();
        var controller = CreateController(service, 8);

        var result = await controller.DeleteAsync(11, cancellationToken);

        Assert.IsType<NoContentResult>(result);
    }

    private static TasksController CreateController(ITaskService service, int userId)
    {
        var controller = new TasksController(service);
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