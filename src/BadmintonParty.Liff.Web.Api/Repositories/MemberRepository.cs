namespace BadmintonParty.Liff.Web.Api.Repositories;

using BadmintonParty.Liff.Web.Api.Contexts;
using BadmintonParty.Liff.Web.Api.Entities;
using BadmintonParty.Liff.Web.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

public class MemberRepository : IMemberRepository
{
    private readonly BadmintonContext _dbContext;

    public MemberRepository(BadmintonContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> CreateMember(string lineUserId, string userName, string pictureUrl)
    {
        if (await _dbContext.Members.AnyAsync(m => m.LineUserId == lineUserId))
        {
            throw new CustomException($"line id: {lineUserId} has existed");
        }

        var newMemberId = Guid.NewGuid().ToString();
        var newMember = new MemberEntity
        {
            MemberId = newMemberId,
            LineUserId = lineUserId,
            MemberName = userName,
            PictureUrl = pictureUrl,
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };

        _dbContext.Members.Add(newMember);
        await _dbContext.SaveChangesAsync();
        return newMemberId;
    }

    public async Task<string> GetMemberIdByLineId(string lineUserId)
    {
        var member = await _dbContext.Members
            .Where(m => m.LineUserId == lineUserId)
            .Select(m => m.MemberId)
            .FirstOrDefaultAsync();

        return member ?? string.Empty;
    }

    public async Task<bool> UpdateMember(MemberEntity entity)
    {
        _dbContext.Members.Update(entity);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<MemberEntity> GetMember(string memberId)
    {
        var member = await _dbContext.Members.FindAsync(memberId);
        if (member is null) throw new CustomException($"找不到MemberId為 [{memberId}] 的使用者");
        return member;
    }

    public IEnumerable<MemberEntity> GetMembers(HashSet<string> memberIds)
    {
        if (memberIds.Count == 0) return Enumerable.Empty<MemberEntity>();

        return _dbContext.Members
            .Where(m => memberIds.Contains(m.MemberId))
            .ToList();
    }

    public Task<List<MemberRecentOpeningEntity>> GetRecentOpenings(string memberId)
        => _dbContext.MemberRecentOpenings
            .Where(ro => ro.MemberId == memberId)
            .OrderByDescending(ro => ro.OpeningTime)
            .ToListAsync();

    public async Task<bool> UpdateRecentOpening(string memberId, string courtId)
    {
        if (string.IsNullOrWhiteSpace(courtId))
        {
            return false;
        }

        var courtExists = await _dbContext.Courts.AnyAsync(c => c.CourtId == courtId);
        if (!courtExists)
        {
            return false;
        }

        var existing = await _dbContext.MemberRecentOpenings
            .FirstOrDefaultAsync(ro => ro.MemberId == memberId && ro.CourtId == courtId);

        if (existing != null)
        {
            existing.OpeningTime = DateTime.UtcNow;
        }
        else
        {
            // Limit to 3 recent
            var recent = await _dbContext.MemberRecentOpenings
                .Where(ro => ro.MemberId == memberId)
                .OrderByDescending(ro => ro.OpeningTime)
                .ToListAsync();

            if (recent.Count >= 3)
            {
                _dbContext.MemberRecentOpenings.RemoveRange(recent.Skip(2));
            }

            _dbContext.MemberRecentOpenings.Add(new MemberRecentOpeningEntity
            {
                MemberId = memberId,
                CourtId = courtId,
                OpeningTime = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }
}
