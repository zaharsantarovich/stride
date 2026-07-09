using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZSLabs.Stride.Domain.Entities;

namespace ZSLabs.Stride.Persistence.Configurations;

public sealed class SpaceConfiguration : IEntityTypeConfiguration<Space>
{
    public void Configure(EntityTypeBuilder<Space> builder)
    {
        builder.ToTable("Spaces");

        builder.HasKey(space => space.Id);

        builder.Property(space => space.Key)
            .IsRequired();

        builder.Property(space => space.Name)
            .IsRequired();

        builder.Property(space => space.CreatedAt)
            .IsRequired();

        builder.HasIndex(space => space.Key)
            .IsUnique();

        builder.HasMany(space => space.Tasks)
            .WithOne(task => task.Space)
            .HasForeignKey(task => task.SpaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}