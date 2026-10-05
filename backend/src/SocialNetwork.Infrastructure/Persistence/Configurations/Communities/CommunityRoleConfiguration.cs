using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Communities;

public class CommunityRoleConfiguration : IEntityTypeConfiguration<CommunityRole>
{
    public void Configure(EntityTypeBuilder<CommunityRole> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => new { x.CommunityId, x.Name }).IsUnique();
        builder.HasOne(x => x.Community).WithMany(x => x.Roles).HasForeignKey(x => x.CommunityId).OnDelete(DeleteBehavior.Cascade);
    }
}
