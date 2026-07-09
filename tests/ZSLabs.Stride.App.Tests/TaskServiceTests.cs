using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ZSLabs.Stride.App.Services;
using ZSLabs.Stride.Domain.Entities;
using ZSLabs.Stride.Domain.Enums;
using ZSLabs.Stride.Persistence;
using TaskEntity = ZSLabs.Stride.Domain.Entities.Task;
using TaskPriority = ZSLabs.Stride.Domain.Enums.TaskPriority;
using TaskStatus = ZSLabs.Stride.Domain.Enums.TaskStatus;

namespace ZSLabs.Stride.App.Tests;

public class TaskServiceTests
{
    [Fact]
    public async global::System.Threading.Tasks.Task CreateTaskDefaultsToBacklog()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", UserRole.Regular, cancellationToken);
        var space = await AddSpaceAsync(context, owner.Id, true, cancellationToken);
        var service = new TaskService(context);

        var task = await service.CreateTaskAsync(space.Id, owner.Id, "Plan sprint", null, null, TaskPriority.High, null, null, cancellationToken);

        Assert.Equal(TaskStatus.Backlog, task.Status);
        Assert.Equal(owner.Id, task.AuthorId);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task GetTasksOrdersByPriorityThenCreatedAtWithinStatus()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", UserRole.Regular, cancellationToken);
        var space = await AddSpaceAsync(context, owner.Id, true, cancellationToken);
        context.Tasks.AddRange(
            new TaskEntity(space.Id, "low", null, TaskStatus.Todo, TaskPriority.Low, owner.Id, null, null, DateTime.UtcNow.AddMinutes(-1)),
            new TaskEntity(space.Id, "critical", null, TaskStatus.Todo, TaskPriority.Critical, owner.Id, null, null, DateTime.UtcNow.AddMinutes(2)),
            new TaskEntity(space.Id, "high-older", null, TaskStatus.Todo, TaskPriority.High, owner.Id, null, null, DateTime.UtcNow.AddMinutes(-3)),
            new TaskEntity(space.Id, "high-newer", null, TaskStatus.Todo, TaskPriority.High, owner.Id, null, null, DateTime.UtcNow.AddMinutes(1)),
            new TaskEntity(space.Id, "backlog", null, TaskStatus.Backlog, TaskPriority.Medium, owner.Id, null, null, DateTime.UtcNow));
        await context.SaveChangesAsync(cancellationToken);

        var service = new TaskService(context);
        var tasks = await service.GetTasksAsync(space.Id, owner.Id, cancellationToken);

        Assert.Equal(new[] { "backlog", "critical", "high-older", "high-newer", "low" }, tasks.Select(task => task.Title));
    }

    [Fact]
    public async global::System.Threading.Tasks.Task UpdateTaskStatusPersistsChange()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", UserRole.Regular, cancellationToken);
        var space = await AddSpaceAsync(context, owner.Id, true, cancellationToken);
        var task = new TaskEntity(space.Id, "Task", null, TaskStatus.Backlog, TaskPriority.Medium, owner.Id, null, null, DateTime.UtcNow);
        context.Tasks.Add(task);
        await context.SaveChangesAsync(cancellationToken);

        var service = new TaskService(context);
        var updated = await service.UpdateTaskStatusAsync(task.Id, owner.Id, TaskStatus.Done, cancellationToken);

        Assert.Equal(TaskStatus.Done, updated.Status);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task CreateTaskRejectsAssigneeWithoutSpaceAccess()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", UserRole.Regular, cancellationToken);
        var otherRegular = await AddUserAsync(context, "other", UserRole.Regular, cancellationToken);
        var privateSpace = await AddSpaceAsync(context, owner.Id, false, cancellationToken);
        var service = new TaskService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateTaskAsync(privateSpace.Id, owner.Id, "Secret", null, null, TaskPriority.Low, otherRegular.Id, null, cancellationToken));
    }

    [Fact]
    public async global::System.Threading.Tasks.Task DeleteTaskCascadesSubtasksAndComments()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", UserRole.Regular, cancellationToken);
        var space = await AddSpaceAsync(context, owner.Id, true, cancellationToken);
        var task = new TaskEntity(space.Id, "Task", null, TaskStatus.Backlog, TaskPriority.Low, owner.Id, null, null, DateTime.UtcNow);
        context.Tasks.Add(task);
        await context.SaveChangesAsync(cancellationToken);

        var subtask = new Subtask(task.Id, "Subtask", null, SubtaskStatus.Todo, owner.Id, null, null, DateTime.UtcNow);
        context.Subtasks.Add(subtask);
        await context.SaveChangesAsync(cancellationToken);

        context.Comments.AddRange(
            new Comment(task.Id, null, owner.Id, "Task comment", DateTime.UtcNow),
            new Comment(null, subtask.Id, owner.Id, "Subtask comment", DateTime.UtcNow));
        await context.SaveChangesAsync(cancellationToken);

        var service = new TaskService(context);
        await service.DeleteTaskAsync(task.Id, owner.Id, cancellationToken);

        Assert.Equal(0, await context.Tasks.CountAsync(cancellationToken));
        Assert.Equal(0, await context.Subtasks.CountAsync(cancellationToken));
        Assert.Equal(0, await context.Comments.CountAsync(cancellationToken));
    }

    private static async Task<User> AddUserAsync(StrideDbContext context, string username, UserRole role, CancellationToken cancellationToken)
    {
        var user = new User(username, username + "-hash", null, role, DateTime.UtcNow);
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);
        return user;
    }

    private static async Task<Space> AddSpaceAsync(StrideDbContext context, int authorId, bool isPublic, CancellationToken cancellationToken)
    {
        var space = new Space($"space-{Guid.NewGuid():N}", "Space", authorId, isPublic, DateTime.UtcNow);
        context.Spaces.Add(space);
        await context.SaveChangesAsync(cancellationToken);
        return space;
    }

    private static StrideDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<StrideDbContext>()
            .UseSqlite(connection)
            .Options;

        return new StrideDbContext(options);
    }
}