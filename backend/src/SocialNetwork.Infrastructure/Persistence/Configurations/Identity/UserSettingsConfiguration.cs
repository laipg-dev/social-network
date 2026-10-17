using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Identity;

public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("UserSettings");

        builder.HasKey(s => s.UserId);

        builder.Property(s => s.Theme)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("light");

        builder.Property(s => s.NotificationEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.Language)
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue("vi");

        builder.Property(s => s.AllowDirectMessagesFrom)
            .HasMaxLength(30)
            .IsRequired()
            .HasDefaultValue("EVERYONE");

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt);

        builder.HasOne(s => s.User)
            .WithOne(u => u.Settings)
            .HasForeignKey<UserSettings>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
