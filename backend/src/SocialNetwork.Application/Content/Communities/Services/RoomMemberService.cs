using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Application.Content.Communities.Requests;
using SocialNetwork.Application.Content.Communities.Responses;
using SocialNetwork.Domain.Communities.Entities;
using SocialNetwork.Domain.Communities.Enums;

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
        await _authorization.RequireRoomPermissionAsync(
            roomId,
            currentUserId,
            CommunityPermissions.RoomMemberView,
            cancellationToken);

        var members = await _roomMemberRepository.GetByRoomAsync(
            roomId,
            cancellationToken);

        return members.Select(ToResponse).ToArray();
    }

    public async Task<RoomMemberResponse> GetMeAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        await _authorization.RequireRoomPermissionAsync(
            roomId,
            currentUserId,
            CommunityPermissions.RoomMemberView,
            cancellationToken);

        var member = await _roomMemberRepository.GetAsync(
            roomId,
            currentUserId,
            cancellationToken)
            ?? throw new NotFoundException("RoomMember", currentUserId);

        return ToResponse(member);
    }

    public async Task<RoomMemberResponse> JoinAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var room = await GetRoomAsync(
            communityId,
            roomId,
            cancellationToken);

        var access = await _authorization.RequireRoomPermissionAsync(
            roomId,
            currentUserId,
            CommunityPermissions.RoomView,
            cancellationToken);

        if (!string.Equals(
                room.Status,
                RoomStatus.Active,
                StringComparison.Ordinal))
        {
            throw new BusinessRuleException(
                "This room is closed and does not accept new members.");
        }

        if (access.IsRoomMember)
        {
            throw new ConflictException(
                "You are already a member of this room.");
        }

        if (!room.IsPublic
            && !access.Permissions.Contains(
                CommunityPermissions.RoomMemberManage))
        {
            throw new ForbiddenException(
                "This room is private. Ask a moderator to add you.");
        }

        await _roomMemberRepository.AddAsync(
            new RoomMember
            {
                RoomId = roomId,
                UserId = currentUserId,
                JoinedAt = DateTime.UtcNow
            },
            cancellationToken);

        var created = await _roomMemberRepository.GetAsync(
            roomId,
            currentUserId,
            cancellationToken)
            ?? throw new NotFoundException("RoomMember", currentUserId);

        return ToResponse(created);
    }

    public async Task LeaveAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        await GetRoomAsync(communityId, roomId, cancellationToken);

        await _authorization.RequireRoomPermissionAsync(
            roomId,
            currentUserId,
            CommunityPermissions.RoomView,
            cancellationToken);

        var member = await _roomMemberRepository.GetAsync(
            roomId,
            currentUserId,
            cancellationToken)
            ?? throw new NotFoundException("RoomMember", currentUserId);

        await _roomMemberRepository.RemoveAsync(
            member,
            cancellationToken);
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

        await _authorization.RequireRoomPermissionAsync(
            roomId,
            currentUserId,
            CommunityPermissions.RoomMemberManage,
            cancellationToken);

        var targetAccess = await _authorization.GetAccessAsync(
            room.CommunityId,
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
                "The user is already a member of this room.");
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
            ?? throw new NotFoundException("RoomMember", request.UserId);

        return ToResponse(created);
    }

    public async Task RemoveAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await GetRoomAsync(communityId, roomId, cancellationToken);

        await _authorization.RequireRoomPermissionAsync(
            roomId,
            currentUserId,
            CommunityPermissions.RoomMemberManage,
            cancellationToken);

        var member = await _roomMemberRepository.GetAsync(
            roomId,
            userId,
            cancellationToken)
            ?? throw new NotFoundException("RoomMember", userId);

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
            ?? throw new NotFoundException("Room", roomId);
    }

    private static RoomMemberResponse ToResponse(RoomMember member)
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
