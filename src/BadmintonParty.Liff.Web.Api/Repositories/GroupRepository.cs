namespace BadmintonParty.Liff.Web.Api.Repositories;

using BadmintonParty.Liff.Web.Api.Contexts;
using BadmintonParty.Liff.Web.Api.Entities;
using BadmintonParty.Liff.Web.Api.Enums;
using BadmintonParty.Liff.Web.Api.Exceptions;
using BadmintonParty.Liff.Web.Api.Models;
using Microsoft.EntityFrameworkCore;

public class GroupRepository : IGroupRepository
{
    private readonly BadmintonContext _dbContext;
    private readonly IMemberGroupRepository _memberGroupRepository;

    public GroupRepository(BadmintonContext dbContext, IMemberGroupRepository memberGroupRepository)
    {
        _dbContext = dbContext;
        _memberGroupRepository = memberGroupRepository;
    }

    public async Task<bool> CloseGroup(string groupId)
    {
        var group = await _dbContext.Groups.FindAsync(groupId);
        if (group is null) return false;

        group.GroupStatus = GroupStatus.Closed;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<GroupEntity> CreateGroup(GroupEntity entity)
    {
        _dbContext.Groups.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<GroupEntity> GetGroupById(string groupId)
    {
        var group = await _dbContext.Groups
            .Include(g => g.Creator)
            .Include(g => g.Members)
                .ThenInclude(gm => gm.Member)
            .FirstOrDefaultAsync(g => g.GroupId == groupId);

        if (group is null) throw new CustomException("查無此羽球團");
        return group;
    }

    public Task<List<GroupEntity>> GetGroupsByCourtId(string courtId)
        => _dbContext.Groups
            .Where(g => g.CourtId == courtId && !g.IsPrivate && g.GroupStatus == GroupStatus.Opened && g.EndTime > DateTime.UtcNow)
            .ToListAsync();

    public IEnumerable<GroupEntity> GetGroupByBatchGetItem(HashSet<string> groupIds)
    {
        if (groupIds.Count == 0) return Enumerable.Empty<GroupEntity>();

        return _dbContext.Groups
            .Include(g => g.Members)
            .Where(g => groupIds.Contains(g.GroupId))
            .ToList();
    }

    public async Task<bool> JoinGroup(string groupId, GroupMember groupMember)
    {
        var group = await _dbContext.Groups.FindAsync(groupId);
        if (group == null) return false;

        var memberGroup = new GroupMemberEntity
        {
            GroupId = groupId,
            MemberId = groupMember.MemberId,
            JoinTime = groupMember.JoinTime == default ? DateTime.UtcNow : groupMember.JoinTime
        };

        _dbContext.GroupMembers.Add(memberGroup);
        await _dbContext.SaveChangesAsync();

        // MemberGroupRepository might need update too
        // await _memberGroupRepository.JoinedGroupAsync(groupMember.MemberId, group.StartTime.ToYearMonthInteger(), group);
        
        return true;
    }

    public async Task<bool> MinusOne(string groupId, string memberId)
    {
        var memberGroup = await _dbContext.GroupMembers
            .Where(gm => gm.GroupId == groupId && gm.MemberId == memberId)
            .OrderByDescending(gm => gm.JoinTime)
            .FirstOrDefaultAsync();

        if (memberGroup == null) return false;

        _dbContext.GroupMembers.Remove(memberGroup);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> LeaveGroup(string groupId, string memberId)
    {
        var memberGroups = await _dbContext.GroupMembers
            .Where(gm => gm.GroupId == groupId && gm.MemberId == memberId)
            .ToListAsync();

        if (memberGroups.Count == 0) return false;

        _dbContext.GroupMembers.RemoveRange(memberGroups);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateGroup(GroupEntity entity)
    {
        var group = await _dbContext.Groups.FindAsync(entity.GroupId);
        if (group == null) return false;

        _dbContext.Entry(group).CurrentValues.SetValues(entity);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
