using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Communities;

public class RoomRolePermissionConfiguration : IEntityTypeConfiguration<RoomRolePermission>
{
    public void Configure(EntityTypeBuilder<RoomRolePermission> builder)
    {
        builder.HasKey(x => new { x.RoomId, x.RoleId, x.PermissionId });
        builder.HasOne(x => x.Room).WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Permission).WithMany().HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
    }
}
