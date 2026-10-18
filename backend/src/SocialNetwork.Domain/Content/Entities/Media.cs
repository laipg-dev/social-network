using SocialNetwork.Domain.Identity.Entities;

namespace SocialNetwork.Domain.Content.Entities;

public class Media
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UploaderId { get; set; }

    public string MediaType { get; set; } = string.Empty; // IMAGE, VIDEO, FILE
    public string Url { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ProcessingStatus { get; set; } = "PENDING"; // PENDING, ATTACHED, FAILED

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User Uploader { get; set; } = null!;
    public ICollection<PostMedia> PostMedia { get; set; } = new List<PostMedia>();
}
