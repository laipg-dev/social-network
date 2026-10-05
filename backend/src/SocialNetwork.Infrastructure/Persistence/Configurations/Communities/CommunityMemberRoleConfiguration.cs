using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Communities;

public class CommunityMemberRoleConfiguration : IEntityTypeConfiguration<CommunityMemberRole>
{
    public void Configure(EntityTypeBuilder<CommunityMemberRole> builder)
    {
        builder.HasKey(x => new { x.CommunityId, x.UserId, x.RoleId });
        builder.HasOne(x => x.CommunityMember).WithMany(x => x.Roles).HasForeignKey(x => new { x.CommunityId, x.UserId }).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Role).WithMany(x => x.Members).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
    }
}
