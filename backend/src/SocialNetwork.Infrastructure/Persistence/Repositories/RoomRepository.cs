using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _dbContext;

    public RoomRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Room?> GetAsync(
        Guid communityId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Rooms.FirstOrDefaultAsync(
            x => x.CommunityId == communityId
                && x.Id == roomId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Room>> GetByCommunityAsync(
        Guid communityId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Rooms
            .Where(x => x.CommunityId == communityId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(
        Guid communityId,
        string name,
        Guid? excludeRoomId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Rooms
            .AsNoTracking()
            .Where(x => x.CommunityId == communityId
                && x.Name == name);

        if (excludeRoomId.HasValue)
        {
            query = query.Where(x => x.Id != excludeRoomId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetMemberCountsAsync(
        Guid communityId,
        CancellationToken cancellationToken = default)
    {
        var counts = await _dbContext.RoomMembers
            .AsNoTracking()
            .Where(x => x.Room.CommunityId == communityId)
            .GroupBy(x => x.RoomId)
            .Select(group => new
            {
                RoomId = group.Key,
                Count = group.Count()
            })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(
            x => x.RoomId,
            x => x.Count);
    }

    public async Task<int> GetMemberCountAsync(
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RoomMembers
            .AsNoTracking()
            .CountAsync(
                x => x.RoomId == roomId,
                cancellationToken);
    }

    public async Task AddAsync(
        Room room,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Rooms.AddAsync(room, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Room room,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Entry(room).State = EntityState.Modified;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Room room,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Rooms.Remove(room);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
