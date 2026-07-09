using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZSLabs.Stride.Domain.Entities;

namespace ZSLabs.Stride.Persistence.Configurations;

public sealed class SubtaskConfiguration : IEntityTypeConfiguration<Subtask>
{
    public void Configure(EntityTypeBuilder<Subtask> builder)
    {
        builder.ToTable("Subtasks");

        builder.HasKey(subtask => subtask.Id);

        builder.Property(subtask => subtask.Title)
            .IsRequired();

        builder.Property(subtask => subtask.CreatedAt)
            .IsRequired();

        builder.HasMany(subtask => subtask.Comments)
            .WithOne(comment => comment.Subtask)
            .HasForeignKey(comment => comment.SubtaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}