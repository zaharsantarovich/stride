using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZSLabs.Stride.Domain.Entities;
using ZSLabs.Stride.Domain.Enums;

namespace ZSLabs.Stride.Persistence;

public static class AdminSeeder
{
    public static async global::System.Threading.Tasks.Task SeedAdminUserAsync(this IServiceProvider services, IConfiguration configuration, CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StrideDbContext>();

        await context.Database.MigrateAsync(cancellationToken);

        if (await context.Users.AnyAsync(user => user.Role == UserRole.Admin, cancellationToken))
        {
            return;
        }

        var username = configuration["Admin:Username"] ?? "admin";
        var password = configuration["Admin:Password"] ?? "ChangeMe123!";
        var email = configuration["Admin:Email"];

        var admin = new User(username, string.Empty, email, UserRole.Admin, DateTime.UtcNow);
        var hasher = new PasswordHasher<User>();
        admin.PasswordHash = hasher.HashPassword(admin, password);

        context.Users.Add(admin);
        await context.SaveChangesAsync(cancellationToken);
    }
}