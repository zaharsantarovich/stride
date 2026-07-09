using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskEntity = ZSLabs.Stride.Domain.Entities.Task;
using TaskPriority = ZSLabs.Stride.Domain.Enums.TaskPriority;
using TaskStatus = ZSLabs.Stride.Domain.Enums.TaskStatus;
using SubtaskStatus = ZSLabs.Stride.Domain.Enums.SubtaskStatus;
using UserRole = ZSLabs.Stride.Domain.Enums.UserRole;
using ZSLabs.Stride.Domain.Entities;
using ZSLabs.Stride.Persistence;

namespace ZSLabs.Stride.Persistence.Tests;

public class StrideDbContextTests
{
    [Fact]
    public async global::System.Threading.Tasks.Task DeleteingASpaceCascadesToTasksSubtasksAndComments()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var user = new User("owner", "hash", "owner@example.com", UserRole.Regular, DateTime.UtcNow);
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        var space = new Space("home", "Home", user.Id, true, DateTime.UtcNow);
        context.Spaces.Add(space);
        await context.SaveChangesAsync(cancellationToken);

        var task = new TaskEntity(space.Id, "Task", null, TaskStatus.Backlog, TaskPriority.Medium, user.Id, null, null, DateTime.UtcNow);
        context.Tasks.Add(task);
        await context.SaveChangesAsync(cancellationToken);

        var subtask = new Subtask(task.Id, "Subtask", null, SubtaskStatus.Todo, user.Id, null, null, DateTime.UtcNow);
        context.Subtasks.Add(subtask);
        await context.SaveChangesAsync(cancellationToken);

        context.Comments.AddRange(
            new Comment(task.Id, null, user.Id, "Task comment", DateTime.UtcNow),
            new Comment(null, subtask.Id, user.Id, "Subtask comment", DateTime.UtcNow));
        await context.SaveChangesAsync(cancellationToken);

        context.Spaces.Remove(space);
        await context.SaveChangesAsync(cancellationToken);

        Assert.Equal(0, await context.Spaces.CountAsync(cancellationToken));
        Assert.Equal(0, await context.Tasks.CountAsync(cancellationToken));
        Assert.Equal(0, await context.Subtasks.CountAsync(cancellationToken));
        Assert.Equal(0, await context.Comments.CountAsync(cancellationToken));
    }

    [Fact]
    public async global::System.Threading.Tasks.Task UniqueConstraintsAreEnforcedForUsernameAndSpaceKey()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var user = new User("duplicate", "hash", null, UserRole.Regular, DateTime.UtcNow);
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        context.Users.Add(new User("duplicate", "hash2", null, UserRole.Regular, DateTime.UtcNow));
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(cancellationToken));

        context.ChangeTracker.Clear();

        var persistedUser = await context.Users.SingleAsync(cancellationToken);
        var firstSpace = new Space("alpha", "Alpha", persistedUser.Id, false, DateTime.UtcNow);
        context.Spaces.Add(firstSpace);
        await context.SaveChangesAsync(cancellationToken);

        context.Spaces.Add(new Space("alpha", "Other", persistedUser.Id, true, DateTime.UtcNow));
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(cancellationToken));
    }

    [Fact]
    public async global::System.Threading.Tasks.Task MaterializedDateTimesAreTaggedAsUtc()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        var createdAt = DateTime.UtcNow;

        await using (var writeContext = CreateContext(connection))
        {
            await writeContext.Database.EnsureCreatedAsync(cancellationToken);
            writeContext.Users.Add(new User("utc-user", "hash", null, UserRole.Regular, createdAt));
            await writeContext.SaveChangesAsync(cancellationToken);
        }

        await using var readContext = CreateContext(connection);
        var user = await readContext.Users.SingleAsync(cancellationToken);

        Assert.Equal(DateTimeKind.Utc, user.CreatedAt.Kind);
    }

    private static StrideDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<StrideDbContext>()
            .UseSqlite(connection)
            .Options;

        return new StrideDbContext(options);
    }
}