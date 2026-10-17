using Microsoft.EntityFrameworkCore;
using SocialNetwork.Domain.Identity.Entities;
using SocialNetwork.Domain.Communities.Entities;
using SocialNetwork.Domain.Content.Entities;
using SocialNetwork.Domain.Messaging.Entities;
using SocialNetwork.Domain.Notification.Entities;
using SocialNetwork.Domain.Moderation.Entities;

namespace SocialNetwork.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailVerification> EmailVerifications => Set<EmailVerification>();
    public DbSet<PasswordReset> PasswordResets => Set<PasswordReset>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();
    public DbSet<UserBlock> UserBlocks => Set<UserBlock>();

    public DbSet<Community> Communities => Set<Community>();
    public DbSet<CommunityMember> CommunityMembers => Set<CommunityMember>();
    public DbSet<CommunityRole> CommunityRoles => Set<CommunityRole>();
    public DbSet<CommunityMemberRole> CommunityMemberRoles => Set<CommunityMemberRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomMember> RoomMembers => Set<RoomMember>();
    public DbSet<RoomRolePermission> RoomRolePermissions => Set<RoomRolePermission>();

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<PostMedia> PostMedia => Set<PostMedia>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<PostReaction> PostReactions => Set<PostReaction>();
    public DbSet<CommentReaction> CommentReactions => Set<CommentReaction>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();

    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMember> ConversationMembers => Set<ConversationMember>();
    public DbSet<DirectMessage> DirectMessages => Set<DirectMessage>();
    public DbSet<RoomMessage> RoomMessages => Set<RoomMessage>();
    public DbSet<DirectMessageAttachment> DirectMessageAttachments => Set<DirectMessageAttachment>();
    public DbSet<RoomMessageAttachment> RoomMessageAttachments => Set<RoomMessageAttachment>();
    public DbSet<MessageReaction> MessageReactions => Set<MessageReaction>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<Report> Reports => Set<Report>();
    public DbSet<CommunityBan> CommunityBans => Set<CommunityBan>();
    public DbSet<ModerationAction> ModerationActions => Set<ModerationAction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
