using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Content.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Content;

public class CommentReactionConfiguration : IEntityTypeConfiguration<CommentReaction>
{
    public void Configure(EntityTypeBuilder<CommentReaction> builder)
    {
        builder.HasKey(x => new { x.CommentId, x.UserId });
        builder.Property(x => x.Type).HasMaxLength(20).IsRequired();
        builder.HasOne(x => x.Comment).WithMany(x => x.Reactions).HasForeignKey(x => x.CommentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
