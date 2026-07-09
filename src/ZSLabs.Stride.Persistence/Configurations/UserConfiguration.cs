using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZSLabs.Stride.Domain.Entities;

namespace ZSLabs.Stride.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Username)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .IsRequired();

        builder.Property(user => user.CreatedAt)
            .IsRequired();

        builder.HasIndex(user => user.Username)
            .IsUnique();

        builder.HasMany(user => user.AuthoredSpaces)
            .WithOne(space => space.Author)
            .HasForeignKey(space => space.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(user => user.AuthoredTasks)
            .WithOne(task => task.Author)
            .HasForeignKey(task => task.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(user => user.AssignedTasks)
            .WithOne(task => task.Assignee)
            .HasForeignKey(task => task.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(user => user.AuthoredSubtasks)
            .WithOne(subtask => subtask.Author)
            .HasForeignKey(subtask => subtask.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(user => user.AssignedSubtasks)
            .WithOne(subtask => subtask.Assignee)
            .HasForeignKey(subtask => subtask.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(user => user.Comments)
            .WithOne(comment => comment.Author)
            .HasForeignKey(comment => comment.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}