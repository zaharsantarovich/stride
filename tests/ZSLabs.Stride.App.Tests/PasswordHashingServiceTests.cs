using ZSLabs.Stride.App.Services;
using ZSLabs.Stride.Domain.Entities;
using ZSLabs.Stride.Domain.Enums;

namespace ZSLabs.Stride.App.Tests;

public class PasswordHashingServiceTests
{
    [Fact]
    public void HashPasswordReturnsHashedValue()
    {
        var service = new PasswordHashingService();
        var user = new User("user", string.Empty, null, UserRole.Regular, DateTime.UtcNow);

        var hash = service.HashPassword(user, "Password123!");

        Assert.NotEqual("Password123!", hash);
        Assert.False(string.IsNullOrWhiteSpace(hash));
    }

    [Fact]
    public void VerifyPasswordReturnsTrueOnlyForMatchingPassword()
    {
        var service = new PasswordHashingService();
        var user = new User("user", string.Empty, null, UserRole.Regular, DateTime.UtcNow);
        var hash = service.HashPassword(user, "Password123!");

        Assert.True(service.VerifyPassword(user, "Password123!", hash));
        Assert.False(service.VerifyPassword(user, "WrongPassword!", hash));
    }
}