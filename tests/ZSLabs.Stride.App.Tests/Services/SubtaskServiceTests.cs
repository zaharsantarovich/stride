using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ZSLabs.Stride.App.Services;
using ZSLabs.Stride.Domain.Entities;
using ZSLabs.Stride.Domain.Enums;
using ZSLabs.Stride.Persistence;
using TaskEntity = ZSLabs.Stride.Domain.Entities.Task;
using TaskPriority = ZSLabs.Stride.Domain.Enums.TaskPriority;
using TaskStatus = ZSLabs.Stride.Domain.Enums.TaskStatus;

namespace ZSLabs.Stride.App.Tests.Services;

public class SubtaskServiceTests
{
    [Fact]
    public async global::System.Threading.Tasks.Task CreateSubtaskAsync_NoStatusProvided_DefaultsToTodo()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", cancellationToken);
        var task = await AddTaskAsync(context, owner.Id, true, cancellationToken);
        var service = new SubtaskService(context);

        var subtask = await service.CreateSubtaskAsync(task.Id, owner.Id, "Subtask", null, null, null, null, cancellationToken);

        Assert.Equal(SubtaskStatus.Todo, subtask.Status);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task UpdateSubtaskAsync_NewStatus_PersistsChange()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", cancellationToken);
        var task = await AddTaskAsync(context, owner.Id, true, cancellationToken);
        var subtask = new Subtask(task.Id, "Subtask", null, SubtaskStatus.Todo, owner.Id, null, null, DateTime.UtcNow);
        context.Subtasks.Add(subtask);
        await context.SaveChangesAsync(cancellationToken);

        var service = new SubtaskService(context);
        var updated = await service.UpdateSubtaskAsync(subtask.Id, owner.Id, "Renamed", null, SubtaskStatus.Done, null, null, cancellationToken);

        Assert.Equal("Renamed", updated.Title);
        Assert.Equal(SubtaskStatus.Done, updated.Status);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task CreateSubtaskAsync_AssigneeWithoutSpaceAccess_ThrowsInvalidOperationException()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", cancellationToken);
        var other = await AddUserAsync(context, "other", cancellationToken);
        var task = await AddTaskAsync(context, owner.Id, false, cancellationToken);
        var service = new SubtaskService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateSubtaskAsync(task.Id, owner.Id, "Subtask", null, null, other.Id, null, cancellationToken));
    }

    [Fact]
    public async global::System.Threading.Tasks.Task CreateSubtaskAsync_NullAssignee_CreatesSubtask()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", cancellationToken);
        var task = await AddTaskAsync(context, owner.Id, false, cancellationToken);
        var service = new SubtaskService(context);

        var subtask = await service.CreateSubtaskAsync(task.Id, owner.Id, "Subtask", null, null, null, null, cancellationToken);

        Assert.Null(subtask.AssigneeId);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task CreateSubtaskAsync_PrivateSpaceOwnerAssignee_CreatesSubtask()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", cancellationToken);
        var task = await AddTaskAsync(context, owner.Id, false, cancellationToken);
        var service = new SubtaskService(context);

        var subtask = await service.CreateSubtaskAsync(task.Id, owner.Id, "Subtask", null, null, owner.Id, null, cancellationToken);

        Assert.Equal(owner.Id, subtask.AssigneeId);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task CreateSubtaskAsync_PublicSpaceRegularUserAssignee_CreatesSubtask()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", cancellationToken);
        var other = await AddUserAsync(context, "other", cancellationToken);
        var task = await AddTaskAsync(context, owner.Id, true, cancellationToken);
        var service = new SubtaskService(context);

        var subtask = await service.CreateSubtaskAsync(task.Id, owner.Id, "Subtask", null, null, other.Id, null, cancellationToken);

        Assert.Equal(other.Id, subtask.AssigneeId);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task CreateSubtaskAsync_AdminAssignee_ThrowsInvalidOperationException()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", cancellationToken);
        var admin = await AddUserAsync(context, "admin", cancellationToken, UserRole.Admin);
        var task = await AddTaskAsync(context, owner.Id, true, cancellationToken);
        var service = new SubtaskService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateSubtaskAsync(task.Id, owner.Id, "Subtask", null, null, admin.Id, null, cancellationToken));
    }

    [Fact]
    public async global::System.Threading.Tasks.Task UpdateSubtaskAsync_AdminAssignee_ThrowsInvalidOperationException()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", cancellationToken);
        var admin = await AddUserAsync(context, "admin", cancellationToken, UserRole.Admin);
        var task = await AddTaskAsync(context, owner.Id, true, cancellationToken);
        var subtask = new Subtask(task.Id, "Subtask", null, SubtaskStatus.Todo, owner.Id, null, null, DateTime.UtcNow);
        context.Subtasks.Add(subtask);
        await context.SaveChangesAsync(cancellationToken);
        var service = new SubtaskService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateSubtaskAsync(subtask.Id, owner.Id, null, null, null, admin.Id, null, cancellationToken));
    }

    [Fact]
    public async global::System.Threading.Tasks.Task DeleteSubtaskAsync_SubtaskWithComments_CascadesDelete()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", cancellationToken);
        var task = await AddTaskAsync(context, owner.Id, true, cancellationToken);
        var subtask = new Subtask(task.Id, "Subtask", null, SubtaskStatus.Todo, owner.Id, null, null, DateTime.UtcNow);
        context.Subtasks.Add(subtask);
        await context.SaveChangesAsync(cancellationToken);
        context.Comments.Add(new Comment(null, subtask.Id, owner.Id, "Comment", DateTime.UtcNow));
        await context.SaveChangesAsync(cancellationToken);

        var service = new SubtaskService(context);
        await service.DeleteSubtaskAsync(subtask.Id, owner.Id, cancellationToken);

        Assert.Equal(0, await context.Subtasks.CountAsync(cancellationToken));
        Assert.Equal(0, await context.Comments.CountAsync(cancellationToken));
    }

    private static async Task<User> AddUserAsync(
        StrideDbContext context,
        string username,
        CancellationToken cancellationToken,
        UserRole role = UserRole.Regular)
    {
        var user = new User(username, username + "-hash", null, role, DateTime.UtcNow);
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);
        return user;
    }

    private static async Task<TaskEntity> AddTaskAsync(StrideDbContext context, int ownerId, bool isPublic, CancellationToken cancellationToken)
    {
        var space = new Space($"space-{Guid.NewGuid():N}", "Space", ownerId, isPublic, DateTime.UtcNow);
        context.Spaces.Add(space);
        await context.SaveChangesAsync(cancellationToken);

        var task = new TaskEntity(space.Id, "Task", null, TaskStatus.Backlog, TaskPriority.Medium, ownerId, null, null, DateTime.UtcNow);
        context.Tasks.Add(task);
        await context.SaveChangesAsync(cancellationToken);
        return task;
    }

    private static StrideDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<StrideDbContext>()
            .UseSqlite(connection)
            .Options;

        return new StrideDbContext(options);
    }
}