using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Content.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Content;

public class MediaConfiguration : IEntityTypeConfiguration<Media>
{
    public void Configure(EntityTypeBuilder<Media> builder)
    {
        builder.ToTable("Media");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.MediaType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.Url)
            .IsRequired();

        builder.Property(m => m.MimeType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.ProcessingStatus)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("PENDING");

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.HasOne(m => m.Uploader)
            .WithMany()
            .HasForeignKey(m => m.UploaderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
