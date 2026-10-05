using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Identity;

public class FriendRequestConfiguration : IEntityTypeConfiguration<FriendRequest>
{
    public void Configure(EntityTypeBuilder<FriendRequest> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
        builder.HasOne(x => x.Sender).WithMany(x => x.SentFriendRequests).HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Receiver).WithMany(x => x.ReceivedFriendRequests).HasForeignKey(x => x.ReceiverId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.SenderId, x.ReceiverId });
    }
}
