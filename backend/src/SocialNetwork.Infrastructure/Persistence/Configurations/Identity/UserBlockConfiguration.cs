using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Identity;

public class UserBlockConfiguration : IEntityTypeConfiguration<UserBlock>
{
    public void Configure(EntityTypeBuilder<UserBlock> builder)
    {
        builder.HasKey(x => new { x.BlockerId, x.BlockedId });
        builder.HasOne(x => x.Blocker).WithMany(x => x.BlockedUsers).HasForeignKey(x => x.BlockerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Blocked).WithMany(x => x.BlockedByUsers).HasForeignKey(x => x.BlockedId).OnDelete(DeleteBehavior.Restrict);
    }
}
