using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskEntity = ZSLabs.Stride.Domain.Entities.Task;
using TaskPriority = ZSLabs.Stride.Domain.Enums.TaskPriority;
using TaskStatus = ZSLabs.Stride.Domain.Enums.TaskStatus;
using UserRole = ZSLabs.Stride.Domain.Enums.UserRole;
using ZSLabs.Stride.App.Services;
using ZSLabs.Stride.Domain.Entities;
using ZSLabs.Stride.Persistence;

namespace ZSLabs.Stride.App.Tests.Services;

public class SpaceServiceTests
{
    [Fact]
    public async global::System.Threading.Tasks.Task CreateSpaceAsync_DuplicateKey_ThrowsInvalidOperationException()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", cancellationToken);
        var service = new SpaceService(context);
        await service.CreateSpaceAsync(owner.Id, "alpha", "Alpha", true, cancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateSpaceAsync(owner.Id, "alpha", "Duplicate", false, cancellationToken));
    }

    [Fact]
    public async global::System.Threading.Tasks.Task GetVisibleSpacesAsync_PrivateAndPublicSpaces_FiltersInaccessiblePrivateSpaces()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", cancellationToken);
        var viewer = await AddUserAsync(context, "viewer", cancellationToken);
        context.Spaces.AddRange(
            new Space("private", "Private", owner.Id, false, DateTime.UtcNow),
            new Space("public", "Public", owner.Id, true, DateTime.UtcNow));
        await context.SaveChangesAsync(cancellationToken);

        var service = new SpaceService(context);
        var visible = await service.GetVisibleSpacesAsync(viewer.Id, cancellationToken);

        Assert.Single(visible);
        Assert.Equal("public", visible[0].Key);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task UpdateSpaceAsync_NonAuthorVisibilityChange_ThrowsUnauthorizedAccessException()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", cancellationToken);
        var other = await AddUserAsync(context, "other", cancellationToken);
        var space = new Space("public", "Public", owner.Id, true, DateTime.UtcNow);
        context.Spaces.Add(space);
        await context.SaveChangesAsync(cancellationToken);

        var service = new SpaceService(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.UpdateSpaceAsync(space.Id, other.Id, "Renamed", false, cancellationToken));

        var updated = await service.UpdateSpaceAsync(space.Id, other.Id, "Renamed", null, cancellationToken);
        Assert.Equal("Renamed", updated.Name);
        Assert.True(updated.IsPublic);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task DeleteSpaceAsync_SpaceWithTasksAndComments_CascadesDelete()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var owner = await AddUserAsync(context, "owner", cancellationToken);
        var space = new Space("alpha", "Alpha", owner.Id, true, DateTime.UtcNow);
        context.Spaces.Add(space);
        await context.SaveChangesAsync(cancellationToken);

        var task = new TaskEntity(space.Id, "Task", null, TaskStatus.Backlog, TaskPriority.Low, owner.Id, null, null, DateTime.UtcNow);
        context.Tasks.Add(task);
        await context.SaveChangesAsync(cancellationToken);
        context.Comments.Add(new Comment(task.Id, null, owner.Id, "Comment", DateTime.UtcNow));
        await context.SaveChangesAsync(cancellationToken);

        var service = new SpaceService(context);
        await service.DeleteSpaceAsync(space.Id, owner.Id, cancellationToken);

        Assert.Equal(0, await context.Spaces.CountAsync(cancellationToken));
        Assert.Equal(0, await context.Tasks.CountAsync(cancellationToken));
        Assert.Equal(0, await context.Comments.CountAsync(cancellationToken));
    }

    private static async Task<User> AddUserAsync(StrideDbContext context, string username, CancellationToken cancellationToken)
    {
        var user = new User(username, username + "-hash", null, UserRole.Regular, DateTime.UtcNow);
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);
        return user;
    }

    private static StrideDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<StrideDbContext>()
            .UseSqlite(connection)
            .Options;

        return new StrideDbContext(options);
    }
}