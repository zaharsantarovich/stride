using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ZSLabs.Stride.Persistence;

public sealed class StrideDbContextFactory : IDesignTimeDbContextFactory<StrideDbContext>
{
    public StrideDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<StrideDbContext>();
        optionsBuilder.UseSqlite("Data Source=stride.db");

        return new StrideDbContext(optionsBuilder.Options);
    }
}