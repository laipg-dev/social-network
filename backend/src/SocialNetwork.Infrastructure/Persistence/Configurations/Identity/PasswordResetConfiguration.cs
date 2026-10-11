using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Identity;

public sealed class PasswordResetConfiguration : IEntityTypeConfiguration<PasswordReset>
{
    public void Configure(EntityTypeBuilder<PasswordReset> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).IsRequired();
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasOne(x => x.User).WithMany(x => x.PasswordResets)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
