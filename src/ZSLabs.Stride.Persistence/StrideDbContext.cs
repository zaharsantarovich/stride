using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ZSLabs.Stride.Domain.Entities;
using TaskEntity = ZSLabs.Stride.Domain.Entities.Task;

namespace ZSLabs.Stride.Persistence;

public class StrideDbContext : DbContext
{
    public StrideDbContext(DbContextOptions<StrideDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Space> Spaces => Set<Space>();

    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();

    public DbSet<Subtask> Subtasks => Set<Subtask>();

    public DbSet<Comment> Comments => Set<Comment>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcNullableDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StrideDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    private sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter()
            : base(
                value => value,
                value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
        {
        }
    }

    private sealed class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
    {
        public UtcNullableDateTimeConverter()
            : base(
                value => value,
                value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value)
        {
        }
    }
}