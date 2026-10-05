using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Moderation.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Moderation;

public class ModerationActionConfiguration : IEntityTypeConfiguration<ModerationAction>
{
    public void Configure(EntityTypeBuilder<ModerationAction> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ActionType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(2000);
        builder.HasOne(x => x.Report).WithMany(x => x.Actions).HasForeignKey(x => x.ReportId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Moderator).WithMany().HasForeignKey(x => x.ModeratorId).OnDelete(DeleteBehavior.Restrict);
    }
}
