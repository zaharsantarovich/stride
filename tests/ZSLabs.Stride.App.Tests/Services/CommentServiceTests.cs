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

public class CommentServiceTests
{
    [Fact]
    public async global::System.Threading.Tasks.Task CommentCreation_TaskAndSubtaskTargets_CreatesComments()
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

        var service = new CommentService(context);
        var taskComment = await service.CreateTaskCommentAsync(task.Id, owner.Id, "Task comment", cancellationToken);
        var subtaskComment = await service.CreateSubtaskCommentAsync(subtask.Id, owner.Id, "Subtask comment", cancellationToken);

        Assert.Equal(task.Id, taskComment.TaskId);
        Assert.Null(taskComment.SubtaskId);
        Assert.Null(subtaskComment.TaskId);
        Assert.Equal(subtask.Id, subtaskComment.SubtaskId);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task CommentModification_NonAuthor_ThrowsUnauthorizedAccessException()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", cancellationToken);
        var other = await AddUserAsync(context, "other", cancellationToken);
        var task = await AddTaskAsync(context, owner.Id, true, cancellationToken);
        var comment = new Comment(task.Id, null, owner.Id, "Original", DateTime.UtcNow);
        context.Comments.Add(comment);
        await context.SaveChangesAsync(cancellationToken);

        var service = new CommentService(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.UpdateCommentAsync(comment.Id, other.Id, "Hijacked", cancellationToken));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.DeleteCommentAsync(comment.Id, other.Id, cancellationToken));
    }

    [Fact]
    public async global::System.Threading.Tasks.Task GetTaskCommentsAsync_MultipleComments_ReturnsAscendingCreationOrder()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", cancellationToken);
        var task = await AddTaskAsync(context, owner.Id, true, cancellationToken);
        context.Comments.AddRange(
            new Comment(task.Id, null, owner.Id, "Second", DateTime.UtcNow.AddMinutes(1)),
            new Comment(task.Id, null, owner.Id, "First", DateTime.UtcNow.AddMinutes(-1)),
            new Comment(task.Id, null, owner.Id, "Third", DateTime.UtcNow.AddMinutes(2)));
        await context.SaveChangesAsync(cancellationToken);

        var service = new CommentService(context);
        var comments = await service.GetTaskCommentsAsync(task.Id, owner.Id, cancellationToken);

        Assert.Equal(new[] { "First", "Second", "Third" }, comments.Select(comment => comment.Content));
    }

    private static async Task<User> AddUserAsync(StrideDbContext context, string username, CancellationToken cancellationToken)
    {
        var user = new User(username, username + "-hash", null, UserRole.Regular, DateTime.UtcNow);
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