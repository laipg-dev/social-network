using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Moderation.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Moderation;

public class CommunityBanConfiguration : IEntityTypeConfiguration<CommunityBan>
{
    public void Configure(EntityTypeBuilder<CommunityBan> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.HasOne(x => x.Community).WithMany().HasForeignKey(x => x.CommunityId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.BannedBy).WithMany().HasForeignKey(x => x.BannedById).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.CommunityId, x.UserId });
    }
}
