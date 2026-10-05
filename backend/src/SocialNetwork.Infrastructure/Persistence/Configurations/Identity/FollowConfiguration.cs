using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Identity;

public class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        builder.HasKey(x => new { x.FollowerId, x.FollowingId });
        builder.HasOne(x => x.Follower).WithMany(x => x.Following).HasForeignKey(x => x.FollowerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Following).WithMany(x => x.Followers).HasForeignKey(x => x.FollowingId).OnDelete(DeleteBehavior.Restrict);
    }
}
