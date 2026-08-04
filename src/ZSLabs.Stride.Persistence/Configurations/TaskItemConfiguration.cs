using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZSLabs.Stride.Domain.Entities;

namespace ZSLabs.Stride.Persistence.Configurations;

public sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(task => task.Id);

        builder.Property(task => task.Title)
            .IsRequired();

        builder.Property(task => task.CreatedAt)
            .IsRequired();

        builder.HasMany(task => task.Subtasks)
            .WithOne(subtask => subtask.Task)
            .HasForeignKey(subtask => subtask.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(task => task.Comments)
            .WithOne(comment => comment.Task)
            .HasForeignKey(comment => comment.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}