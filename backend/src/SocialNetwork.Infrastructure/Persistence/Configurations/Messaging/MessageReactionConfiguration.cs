using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Messaging.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Messaging;

public class MessageReactionConfiguration : IEntityTypeConfiguration<MessageReaction>
{
    public void Configure(EntityTypeBuilder<MessageReaction> builder)
    {
        builder.HasKey(x => new { x.MessageId, x.UserId });
        builder.Property(x => x.Type).HasMaxLength(20).IsRequired();
        builder.HasOne(x => x.Message).WithMany(x => x.Reactions).HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
