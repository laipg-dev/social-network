using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetwork.Domain.Messaging.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Configurations.Messaging;

public class DirectMessageAttachmentConfiguration : IEntityTypeConfiguration<DirectMessageAttachment>
{
    public void Configure(EntityTypeBuilder<DirectMessageAttachment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Url).IsRequired();
        builder.Property(x => x.FileType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(255);
        builder.HasOne(x => x.DirectMessage).WithMany(x => x.Attachments).HasForeignKey(x => x.DirectMessageId).OnDelete(DeleteBehavior.Cascade);
    }
}
