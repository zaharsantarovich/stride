using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ZSLabs.Stride.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Stride") ?? "Data Source=stride.db";

        services.AddDbContext<StrideDbContext>(options =>
            options.UseSqlite(connectionString, sqlite => sqlite.MigrationsAssembly(typeof(StrideDbContext).Assembly.FullName)));

        return services;
    }
}