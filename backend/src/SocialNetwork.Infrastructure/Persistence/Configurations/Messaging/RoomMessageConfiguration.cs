using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Messaging.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Messaging;

public class RoomMessageConfiguration : IEntityTypeConfiguration<RoomMessage>
{
    public void Configure(EntityTypeBuilder<RoomMessage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).HasMaxLength(5000);
        builder.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Sender).WithMany().HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ReplyToMessage).WithMany(x => x.Replies).HasForeignKey(x => x.ReplyToMessageId).OnDelete(DeleteBehavior.Restrict);
    }
}
