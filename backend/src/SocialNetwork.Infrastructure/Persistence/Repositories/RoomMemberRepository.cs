using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Content.Communities.Interfaces;
using SocialNetwork.Domain.Communities.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Repositories;

public class RoomMemberRepository : IRoomMemberRepository
{
    private readonly AppDbContext _dbContext;

    public RoomMemberRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoomMember?> GetAsync(
        Guid roomId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RoomMembers
            .Include(x => x.User)
            .ThenInclude(x => x.Profile)
            .FirstOrDefaultAsync(
                x => x.RoomId == roomId
                    && x.UserId == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<RoomMember>> GetByRoomAsync(
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RoomMembers
            .AsNoTracking()
            .Include(x => x.User)
            .ThenInclude(x => x.Profile)
            .Where(x => x.RoomId == roomId)
            .OrderBy(x => x.JoinedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        RoomMember member,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.RoomMembers.AddAsync(
            member,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(
        RoomMember member,
        CancellationToken cancellationToken = default)
    {
        _dbContext.RoomMembers.Remove(member);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
