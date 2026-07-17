using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ZSLabs.Stride.App.Services;
using ZSLabs.Stride.Domain.Entities;
using ZSLabs.Stride.Domain.Enums;
using ZSLabs.Stride.Persistence;

namespace ZSLabs.Stride.App.Tests.Services;

public class UserServiceTests
{
    [Fact]
    public async global::System.Threading.Tasks.Task GetRegularUsersAsync_AllUsers_ExcludesAdminsAndOrdersByUsername()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        context.Users.AddRange(
            new User("zoe", "hash", null, UserRole.Regular, DateTime.UtcNow),
            new User("admin", "hash", null, UserRole.Admin, DateTime.UtcNow),
            new User("anna", "hash", null, UserRole.Regular, DateTime.UtcNow));
        await context.SaveChangesAsync(cancellationToken);

        var service = new UserService(context, new PasswordHashingService());

        var users = await service.GetRegularUsersAsync(cancellationToken);

        Assert.Equal(["anna", "zoe"], users.Select(user => user.Username));
        Assert.All(users, user => Assert.Equal(UserRole.Regular, user.Role));
    }

    [Fact]
    public async global::System.Threading.Tasks.Task CreateRegularUserAsync_NewUser_PersistsHashedPassword()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var service = new UserService(context, new PasswordHashingService());

        var user = await service.CreateRegularUserAsync("maria", "Password123!", "maria@example.com", cancellationToken);

        Assert.Equal(UserRole.Regular, user.Role);
        Assert.NotEqual("Password123!", user.PasswordHash);
        Assert.Equal(1, await context.Users.CountAsync(cancellationToken));
    }

    [Fact]
    public async global::System.Threading.Tasks.Task UpdateRegularUserAsync_AllFields_UpdatesFieldsAndPassword()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var hashingService = new PasswordHashingService();
        var seeded = new User("maria", string.Empty, "old@example.com", UserRole.Regular, DateTime.UtcNow);
        seeded.PasswordHash = hashingService.HashPassword(seeded, "OldPassword123!");
        context.Users.Add(seeded);
        await context.SaveChangesAsync(cancellationToken);

        var service = new UserService(context, hashingService);

        var updated = await service.UpdateRegularUserAsync(seeded.Id, "maria2", "NewPassword123!", "new@example.com", cancellationToken);

        Assert.Equal("maria2", updated.Username);
        Assert.Equal("new@example.com", updated.Email);
        Assert.True(hashingService.VerifyPassword(updated, "NewPassword123!", updated.PasswordHash));
    }

    [Fact]
    public async global::System.Threading.Tasks.Task CreateRegularUserAsync_DuplicateUsername_ThrowsInvalidOperationException()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var hashingService = new PasswordHashingService();
        var existing = new User("taken", string.Empty, null, UserRole.Regular, DateTime.UtcNow);
        existing.PasswordHash = hashingService.HashPassword(existing, "Password123!");
        context.Users.Add(existing);
        await context.SaveChangesAsync(cancellationToken);

        var service = new UserService(context, hashingService);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateRegularUserAsync("taken", "OtherPassword123!", null, cancellationToken));
    }

    private static StrideDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<StrideDbContext>()
            .UseSqlite(connection)
            .Options;

        return new StrideDbContext(options);
    }
}