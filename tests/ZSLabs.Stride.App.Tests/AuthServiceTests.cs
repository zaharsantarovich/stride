using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ZSLabs.Stride.App.Services;
using ZSLabs.Stride.Domain.Enums;
using ZSLabs.Stride.Persistence;
using User = ZSLabs.Stride.Domain.Entities.User;

namespace ZSLabs.Stride.App.Tests;

public class AuthServiceTests
{
    [Fact]
    public async global::System.Threading.Tasks.Task AuthenticateAsync_ValidCredentials_ReturnsUser()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var passwordHashingService = new PasswordHashingService();
        var user = new User("jane", string.Empty, "jane@example.com", UserRole.Regular, DateTime.UtcNow);
        user.PasswordHash = passwordHashingService.HashPassword(user, "Password123!");
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        var authService = new AuthService(context, passwordHashingService);

        var result = await authService.AuthenticateAsync("jane", "Password123!", cancellationToken);

        Assert.NotNull(result);
        Assert.Equal("jane", result.Username);
    }

    [Fact]
    public async global::System.Threading.Tasks.Task AuthenticateAsync_InvalidCredentials_ReturnsNull()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var passwordHashingService = new PasswordHashingService();
        var user = new User("jane", string.Empty, null, UserRole.Regular, DateTime.UtcNow);
        user.PasswordHash = passwordHashingService.HashPassword(user, "Password123!");
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        var authService = new AuthService(context, passwordHashingService);

        var wrongPassword = await authService.AuthenticateAsync("jane", "WrongPassword!", cancellationToken);
        var wrongUsername = await authService.AuthenticateAsync("missing", "Password123!", cancellationToken);

        Assert.Null(wrongPassword);
        Assert.Null(wrongUsername);
    }

    private static StrideDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<StrideDbContext>()
            .UseSqlite(connection)
            .Options;

        return new StrideDbContext(options);
    }
}