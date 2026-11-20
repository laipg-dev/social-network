using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Application.Content.Communities.Requests;
using SocialNetwork.Application.Content.Communities.Responses;
using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Application.Content.Communities.Services;

public class RoomMemberService : IRoomMemberService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IRoomMemberRepository _roomMemberRepository;
    private readonly ICommunityAuthorization _authorization;

    public RoomMemberService(
        IRoomRepository roomRepository,
        IRoomMemberRepository roomMemberRepository,
        ICommunityAuthorization authorization)
    {
        _roomRepository = roomRepository;
        _roomMemberRepository = roomMemberRepository;
        _authorization = authorization;
    }

    public async Task<IReadOnlyList<RoomMemberResponse>> GetMembersAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var room = await GetRoomAsync(
            communityId,
            roomId,
            cancellationToken);

        await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            Domain.Communities.Enums.CommunityPermissions.RoomMemberView,
            cancellationToken);

        // Public Room không cần access grant.
        if (room.IsPublic)
        {
            return [];
        }

        var members = await _roomMemberRepository.GetByRoomAsync(
            roomId,
            cancellationToken);

        return members
            .Select(ToResponse)
            .ToArray();
    }

    public async Task<RoomMemberResponse> AddAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        AddRoomMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var room = await GetRoomAsync(
            communityId,
            roomId,
            cancellationToken);

        await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            Domain.Communities.Enums.CommunityPermissions.RoomMemberManage,
            cancellationToken);

        if (room.IsPublic)
        {
            throw new BusinessRuleException(
                "Public rooms do not require access grants.");
        }

        var targetAccess = await _authorization.GetAccessAsync(
            communityId,
            request.UserId,
            cancellationToken);

        if (!targetAccess.IsMember)
        {
            throw new BusinessRuleException(
                "The user is not an active member of this community.");
        }

        var existing = await _roomMemberRepository.GetAsync(
            roomId,
            request.UserId,
            cancellationToken);

        if (existing is not null)
        {
            throw new ConflictException(
                "The user already has access to this room.");
        }

        await _roomMemberRepository.AddAsync(
            new RoomMember
            {
                RoomId = roomId,
                UserId = request.UserId,
                JoinedAt = DateTime.UtcNow
            },
            cancellationToken);

        var created = await _roomMemberRepository.GetAsync(
            roomId,
            request.UserId,
            cancellationToken)
            ?? throw new NotFoundException(
                "RoomMember",
                request.UserId);

        return ToResponse(created);
    }

    public async Task RemoveAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var room = await GetRoomAsync(
            communityId,
            roomId,
            cancellationToken);

        await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            Domain.Communities.Enums.CommunityPermissions.RoomMemberManage,
            cancellationToken);

        if (room.IsPublic)
        {
            throw new BusinessRuleException(
                "Public rooms do not require access grants.");
        }

        var member = await _roomMemberRepository.GetAsync(
            roomId,
            userId,
            cancellationToken)
            ?? throw new NotFoundException(
                "RoomMember",
                userId);

        await _roomMemberRepository.RemoveAsync(
            member,
            cancellationToken);
    }

    private async Task<Room> GetRoomAsync(
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken)
    {
        return await _roomRepository.GetAsync(
            communityId,
            roomId,
            cancellationToken)
            ?? throw new NotFoundException(
                "Room",
                roomId);
    }

    private static RoomMemberResponse ToResponse(
        RoomMember member)
    {
        return new RoomMemberResponse
        {
            UserId = member.UserId,
            Username = member.User?.Username ?? string.Empty,
            DisplayName = member.User?.Profile?.DisplayName,
            AvatarUrl = member.User?.Profile?.AvatarUrl,
            JoinedAt = member.JoinedAt
        };
    }
}