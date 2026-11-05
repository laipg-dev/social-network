using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Communities.Entities;
using SocialNetwork.Domain.Communities.Enums;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Communities;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Type).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue(RoomStatus.Active);
        builder.Property(x => x.IsPublic).HasDefaultValue(true);
        builder.HasIndex(x => new { x.CommunityId, x.Name }).IsUnique();
        builder.HasIndex(x => new { x.CommunityId, x.Position });
        builder.HasOne(x => x.Community).WithMany(x => x.Rooms).HasForeignKey(x => x.CommunityId).OnDelete(DeleteBehavior.Cascade);
    }
}
