using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Identity;

public sealed class EmailVerificationConfiguration : IEntityTypeConfiguration<EmailVerification>
{
    public void Configure(EntityTypeBuilder<EmailVerification> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).IsRequired();
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasOne(x => x.User).WithMany(x => x.EmailVerifications)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
