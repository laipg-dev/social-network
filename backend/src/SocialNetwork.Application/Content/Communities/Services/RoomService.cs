using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Content.Communities.Authorization;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Application.Content.Communities.Requests;
using SocialNetwork.Application.Content.Communities.Responses;
using SocialNetwork.Domain.Communities.Entities;
using SocialNetwork.Domain.Communities.Enums;

namespace SocialNetwork.Application.Content.Communities.Services;

public class RoomService : IRoomService
{
    private readonly ICommunityRepository _communityRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly ICommunityAuthorization _authorization;

    public RoomService(
        ICommunityRepository communityRepository,
        IRoomRepository roomRepository,
        ICommunityAuthorization authorization)
    {
        _communityRepository = communityRepository;
        _roomRepository = roomRepository;
        _authorization = authorization;
    }

    public async Task<IReadOnlyList<RoomResponse>> GetRoomsAsync(
    Guid currentUserId,
    Guid communityId,
    CancellationToken cancellationToken = default)
    {
        var communityAccess = await _authorization.GetAccessAsync(
            communityId,
            currentUserId,
            cancellationToken);

        if (!communityAccess.IsMember)
        {
            throw new ForbiddenException(
                "You are not a member of this community.");
        }

        var rooms = await _roomRepository.GetByCommunityAsync(
            communityId,
            cancellationToken);

        var accesses = await _authorization.GetRoomAccessesAsync(
            communityId,
            currentUserId,
            rooms.Select(x => x.Id).ToArray(),
            cancellationToken);

        var counts = await _roomRepository.GetMemberCountsAsync(
            communityId,
            cancellationToken);

        return rooms
            .Where(room => accesses.TryGetValue(room.Id, out var access)
                && access.CanView)
            .OrderBy(room => room.Position)
            .ThenBy(room => room.Name, StringComparer.Ordinal)
            .Select(room => ToResponse(
                room,
                room.IsPublic
                    ? 0
                    : counts.TryGetValue(room.Id, out var count)
                        ? count
                        : 0,
                accesses[room.Id]))
            .ToArray();
    }

    public async Task<RoomResponse> GetRoomAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var room = await GetRoomEntityAsync(
            communityId,
            roomId,
            cancellationToken);

        var access = await _authorization.GetRoomAccessAsync(
            roomId,
            currentUserId,
            cancellationToken);

        // Không có quyền truy cập Room.
        // Trả về 404 để tránh tiết lộ Room cho người không có quyền.
        if (!access.CanView)
        {
            throw new NotFoundException("Room", roomId);
        }

        var memberCount = await _roomRepository.GetMemberCountAsync(
            roomId,
            cancellationToken);

        return ToResponse(
            room,
            room.IsPublic ? 0 : memberCount,
            access);
    }

    public async Task<RoomResponse> CreateAsync(
        Guid currentUserId,
        Guid communityId,
        CreateRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        var access = await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.RoomCreate,
            cancellationToken);

        var community = await _communityRepository.GetByIdAsync(
            communityId,
            cancellationToken)
            ?? throw new NotFoundException("Community", communityId);

        if (!string.Equals(
                community.Status,
                CommunityStatus.Active,
                StringComparison.Ordinal))
        {
            throw new BusinessRuleException(
                "This community is closed and cannot create rooms.");
        }

        var name = NormalizeName(request.Name);
        var type = NormalizeType(request.Type);

        if (await _roomRepository.NameExistsAsync(
                communityId,
                name,
                cancellationToken: cancellationToken))
        {
            throw new ConflictException(
                $"Room '{name}' already exists in this community.");
        }

        var rooms = await _roomRepository.GetByCommunityAsync(
            communityId,
            cancellationToken);

        var position = request.Position
            ?? (rooms.Count == 0 ? 0 : rooms.Max(x => x.Position) + 1);

        var room = new Room
        {
            Id = Guid.NewGuid(),
            CommunityId = communityId,
            Name = name,
            Description = NormalizeOptional(request.Description),
            Type = type,
            Position = position,
            IsPublic = request.IsPublic,
            Status = RoomStatus.Active
        };

        // Mọi room đều có RoomMember row; creator được thêm ngay.
        // Chỉ Private Room mới cần access grant.
        // Public Room mặc định cho phép thành viên Community truy cập.
        if (!room.IsPublic)
        {
            room.Members.Add(new RoomMember
            {
                RoomId = room.Id,
                UserId = currentUserId,
                JoinedAt = DateTime.UtcNow
            });
        }

        await _roomRepository.AddAsync(room, cancellationToken);

        return ToResponse(
            room,
            memberCount: room.IsPublic ? 0 : 1,
            new RoomAccess
            {
                CanView = true,
                IsRoomMember = !room.IsPublic,
                Permissions = access.Permissions
            });
    }
    public async Task<RoomResponse> UpdateAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        UpdateRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        var room = await GetRoomEntityAsync(
            communityId,
            roomId,
            cancellationToken);

        var access = await _authorization.RequireRoomPermissionAsync(
            roomId,
            currentUserId,
            CommunityPermissions.RoomManage,
            cancellationToken);

        var name = NormalizeName(request.Name);
        var type = NormalizeType(request.Type);

        if (await _roomRepository.NameExistsAsync(
                communityId,
                name,
                room.Id,
                cancellationToken))
        {
            throw new ConflictException(
                $"Room '{name}' already exists in this community.");
        }

        room.Name = name;
        room.Description = NormalizeOptional(request.Description);
        room.Type = type;
        room.IsPublic = request.IsPublic;
        room.Position = request.Position;
        room.UpdatedAt = DateTime.UtcNow;

        await _roomRepository.UpdateAsync(room, cancellationToken);

        var memberCount = await _roomRepository.GetMemberCountAsync(
            roomId,
            cancellationToken);

        return ToResponse(
            room,
            room.IsPublic ? 0 : memberCount,
            access);
    }

    public async Task<RoomResponse> UpdateStatusAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        UpdateRoomStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var status = NormalizeStatus(request.Status);

        var room = await GetRoomEntityAsync(
            communityId,
            roomId,
            cancellationToken);

        var access = await _authorization.RequireRoomPermissionAsync(
             roomId,
             currentUserId,
             CommunityPermissions.RoomManage,
             cancellationToken);

        if (!string.Equals(
                room.Status,
                status,
                StringComparison.Ordinal))
        {
            room.Status = status;
            room.UpdatedAt = DateTime.UtcNow;

            await _roomRepository.UpdateAsync(room, cancellationToken);
        }

        var memberCount = await _roomRepository.GetMemberCountAsync(
            roomId,
            cancellationToken);

        return ToResponse(
            room,
            room.IsPublic ? 0 : memberCount,
            access);
    }

    public async Task DeleteAsync(
        Guid currentUserId,
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var room = await GetRoomEntityAsync(
            communityId,
            roomId,
            cancellationToken);

        var access = await _authorization.GetAccessAsync(
            communityId,
            currentUserId,
            cancellationToken);

        if (!access.IsMember)
        {
            throw new ForbiddenException(
                "You are not a member of this community.");
        }

        // Owner hoặc người có ROOM_MANAGE / ROOM_DELETE
        // được phép xóa Room.
        if (!access.IsOwner
            && !access.Permissions.Contains(
                CommunityPermissions.RoomManage)
            && !access.Permissions.Contains(
                CommunityPermissions.RoomDelete))
        {
            throw new ForbiddenException(
                "You do not have permission to delete this room.");
        }

        await _roomRepository.DeleteAsync(
            room,
            cancellationToken);
    }

    public async Task<IReadOnlyList<RoomResponse>> ReorderAsync(
        Guid currentUserId,
        Guid communityId,
        ReorderRoomsRequest request,
        CancellationToken cancellationToken = default)
    {
        await _authorization.RequireCommunityPermissionAsync(
            communityId,
            currentUserId,
            CommunityPermissions.RoomManage,
            cancellationToken);

        var rooms = await _roomRepository.GetByCommunityAsync(
            communityId,
            cancellationToken);

        var roomsById = rooms.ToDictionary(x => x.Id);
        var items = request.Items;

        if (items.Select(x => x.RoomId).Distinct().Count() != items.Count)
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    ["Items"] = ["RoomId must be unique in the request."]
                });
        }

        if (items.Select(x => x.Position).Distinct().Count() != items.Count)
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    ["Items"] = ["Position must be unique in the request."]
                });
        }

        foreach (var item in items)
        {
            if (!roomsById.TryGetValue(item.RoomId, out var room))
            {
                throw new NotFoundException("Room", item.RoomId);
            }

            room.Position = item.Position;
        }

        await _roomRepository.SaveChangesAsync(cancellationToken);

        var accesses = await _authorization.GetRoomAccessesAsync(
            communityId,
            currentUserId,
            rooms.Select(x => x.Id).ToArray(),
            cancellationToken);

        var counts = await _roomRepository.GetMemberCountsAsync(
            communityId,
            cancellationToken);

        return rooms
            .Where(room => accesses.TryGetValue(room.Id, out var access)
                && access.CanView)
            .OrderBy(room => room.Position)
            .ThenBy(room => room.Name, StringComparer.Ordinal)
            .Select(room => ToResponse(
                room,
                room.IsPublic
                    ? 0
                    : counts.TryGetValue(room.Id, out var count)
                        ? count
                        : 0,
                accesses[room.Id]))
            .ToArray();
    }

    private async Task<Room> GetRoomEntityAsync(
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

    private static string NormalizeName(string? name)
    {
        var normalized = name?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    [nameof(CreateRoomRequest.Name)] =
                        ["Name is required."]
                });
        }

        return normalized;
    }

    private static string NormalizeType(string? type)
    {
        var normalized = type?.Trim().ToUpperInvariant()
            ?? string.Empty;

        if (!RoomType.All.Contains(normalized))
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    [nameof(CreateRoomRequest.Type)] =
                        [$"Type must be one of: {string.Join(", ", RoomType.All)}."]
                });
        }

        return normalized;
    }

    private static string NormalizeStatus(string? status)
    {
        var normalized = status?.Trim().ToUpperInvariant()
            ?? string.Empty;

        if (!RoomStatus.All.Contains(normalized))
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    [nameof(UpdateRoomStatusRequest.Status)] =
                        [$"Status must be one of: {string.Join(", ", RoomStatus.All)}."]
                });
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();

        return string.IsNullOrEmpty(normalized)
            ? null
            : normalized;
    }

    private static RoomResponse ToResponse(
        Room room,
        int memberCount,
        RoomAccess access)
    {
        return new RoomResponse
        {
            Id = room.Id,
            CommunityId = room.CommunityId,
            Name = room.Name,
            Description = room.Description,
            Type = room.Type,
            Position = room.Position,
            IsPublic = room.IsPublic,
            Status = room.Status,
            MemberCount = memberCount,
            IsRoomMember = access.IsRoomMember,
            MyPermissions = access.Permissions,
            CreatedAt = room.CreatedAt,
            UpdatedAt = room.UpdatedAt
        };
    }
}
