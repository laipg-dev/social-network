using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Content.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Content;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).HasMaxLength(5000).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
        builder.HasOne(x => x.Post).WithMany(x => x.Comments).HasForeignKey(x => x.PostId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Author).WithMany().HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ParentComment).WithMany(x => x.Replies).HasForeignKey(x => x.ParentCommentId).OnDelete(DeleteBehavior.Restrict);
    }
}
