using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZSLabs.Stride.Domain.Entities;

namespace ZSLabs.Stride.Persistence.Configurations;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Comments_ExactlyOneOwner",
                "([TaskId] IS NOT NULL AND [SubtaskId] IS NULL) OR ([TaskId] IS NULL AND [SubtaskId] IS NOT NULL)");
        });

        builder.HasKey(comment => comment.Id);

        builder.Property(comment => comment.Content)
            .IsRequired();

        builder.Property(comment => comment.CreatedAt)
            .IsRequired();
    }
}