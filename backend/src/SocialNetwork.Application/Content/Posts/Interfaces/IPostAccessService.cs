namespace SocialNetwork.Application.Content.Posts.Interfaces;
public interface IPostAccessService { Task EnsureCanViewAsync(Guid postId, Guid? viewerId, CancellationToken ct); }
