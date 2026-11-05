using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Repositories;

public class RoomPermissionRepository : IRoomPermissionRepository
{
    private readonly AppDbContext _dbContext;

    public RoomPermissionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RoomRolePermission>> GetByRoomAsync(
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RoomRolePermissions
            .AsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.Permission)
            .Where(x => x.RoomId == roomId)
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceRoleOverridesAsync(
        Guid roomId,
        Guid roleId,
        IReadOnlyCollection<(Guid PermissionId, bool IsAllowed)> overrides,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.RoomRolePermissions
            .Where(x => x.RoomId == roomId
                && x.RoleId == roleId)
            .ToListAsync(cancellationToken);

        _dbContext.RoomRolePermissions.RemoveRange(existing);

        foreach (var (permissionId, isAllowed) in overrides)
        {
            _dbContext.RoomRolePermissions.Add(new RoomRolePermission
            {
                RoomId = roomId,
                RoleId = roleId,
                PermissionId = permissionId,
                IsAllowed = isAllowed
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveOverrideAsync(
        Guid roomId,
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.RoomRolePermissions
            .FirstOrDefaultAsync(
                x => x.RoomId == roomId
                    && x.RoleId == roleId
                    && x.PermissionId == permissionId,
                cancellationToken);

        if (entity is null)
        {
            return;
        }

        _dbContext.RoomRolePermissions.Remove(entity);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
