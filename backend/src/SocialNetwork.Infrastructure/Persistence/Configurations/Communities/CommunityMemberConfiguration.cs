using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Communities.Entities;
using SocialNetwork.Domain.Communities.Enums;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Communities;

public class CommunityMemberConfiguration : IEntityTypeConfiguration<CommunityMember>
{
    public void Configure(EntityTypeBuilder<CommunityMember> builder)
    {
        builder.HasKey(x => new { x.CommunityId, x.UserId });
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue(CommunityMemberStatus.Active);
        builder.HasIndex(x => new { x.CommunityId, x.Status });
        builder.HasOne(x => x.Community).WithMany(x => x.Members).HasForeignKey(x => x.CommunityId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
