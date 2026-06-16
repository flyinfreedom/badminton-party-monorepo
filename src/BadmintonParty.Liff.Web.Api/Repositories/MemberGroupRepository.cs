namespace BadmintonParty.Liff.Web.Api.Repositories;

using BadmintonParty.Liff.Web.Api.Contexts;
using BadmintonParty.Liff.Web.Api.Entities;
using BadmintonParty.Liff.Web.Api.Enums;
using Microsoft.EntityFrameworkCore;

public class MemberGroupRepository : IMemberGroupRepository
{
    private readonly BadmintonContext _dbContext;

    public MemberGroupRepository(BadmintonContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<MemberGroupEntity>> GetCurrentMemberGroupAsync(string memberId, int currentYearMonth)
    {
        // In relational, we can just query all groups for this member from currentYearMonth onwards.
        // We might need to group them by YearMonth to match the original structure if needed, 
        // but let's see how it's used.
        
        var startDate = new DateTime(currentYearMonth / 100, currentYearMonth % 100, 1);

        var createdGroups = await _dbContext.Groups
            .Where(g => g.MemberId == memberId && g.StartTime >= startDate)
            .ToListAsync();

        var joinedGroups = await _dbContext.GroupMembers
            .Include(gm => gm.Group)
            .Where(gm => gm.MemberId == memberId && gm.Group.StartTime >= startDate)
            .Select(gm => gm.Group)
            .ToListAsync();

        // Group by YearMonth
        var allYearMonths = createdGroups.Select(g => g.StartTime.Year * 100 + g.StartTime.Month)
            .Union(joinedGroups.Select(g => g.StartTime.Year * 100 + g.StartTime.Month))
            .Distinct()
            .OrderBy(ym => ym);

        var result = new List<MemberGroupEntity>();
        foreach (var ym in allYearMonths)
        {
            var ymStartDate = new DateTime(ym / 100, ym % 100, 1);
            var ymEndDate = ymStartDate.AddMonths(1);

            result.Add(new MemberGroupEntity
            {
                MemberId = memberId,
                GroupStartYearMonth = ym,
                CreatedGroups = createdGroups
                    .Where(g => g.StartTime >= ymStartDate && g.StartTime < ymEndDate)
                    .Select(g => new GroupInfo
                    {
                        GroupId = g.GroupId,
                        StartTime = g.StartTime,
                        EndTime = g.EndTime,
                        GroupStatus = g.GroupStatus
                    }).ToList(),
                JoinedGroups = joinedGroups
                    .Where(g => g.StartTime >= ymStartDate && g.StartTime < ymEndDate)
                    .Select(g => new GroupInfo
                    {
                        GroupId = g.GroupId,
                        StartTime = g.StartTime,
                        EndTime = g.EndTime,
                        GroupStatus = g.GroupStatus
                    }).ToList()
            });
        }

        return result;
    }

    public async Task<MemberGroupEntity?> GetMemberGroupAsync(string memberId, int currentYearMonth)
    {
        var ymStartDate = new DateTime(currentYearMonth / 100, currentYearMonth % 100, 1);
        var ymEndDate = ymStartDate.AddMonths(1);

        var createdGroups = await _dbContext.Groups
            .Where(g => g.MemberId == memberId && g.StartTime >= ymStartDate && g.StartTime < ymEndDate)
            .ToListAsync();

        var joinedGroups = await _dbContext.GroupMembers
            .Include(gm => gm.Group)
            .Where(gm => gm.MemberId == memberId && gm.Group.StartTime >= ymStartDate && gm.Group.StartTime < ymEndDate)
            .Select(gm => gm.Group)
            .ToListAsync();

        if (createdGroups.Count == 0 && joinedGroups.Count == 0) return null;

        return new MemberGroupEntity
        {
            MemberId = memberId,
            GroupStartYearMonth = currentYearMonth,
            CreatedGroups = createdGroups.Select(g => new GroupInfo
            {
                GroupId = g.GroupId,
                StartTime = g.StartTime,
                EndTime = g.EndTime,
                GroupStatus = g.GroupStatus
            }).ToList(),
            JoinedGroups = joinedGroups.Select(g => new GroupInfo
            {
                GroupId = g.GroupId,
                StartTime = g.StartTime,
                EndTime = g.EndTime,
                GroupStatus = g.GroupStatus
            }).ToList()
        };
    }

    // In relational model, these "Update/Close" methods might be redundant if the 
    // source of truth (Groups/GroupMembers table) is already updated.
    // However, to keep the interface consistent, we can just return the updated view.

    public async Task<MemberGroupEntity> CreateGroupAsync(string memberId, int groupStartYearMonth, GroupEntity group)
    {
        // The group is already created in GroupRepository. Just return the current state.
        return await GetMemberGroupAsync(memberId, groupStartYearMonth) ?? new MemberGroupEntity { MemberId = memberId, GroupStartYearMonth = groupStartYearMonth };
    }

    public async Task<MemberGroupEntity> JoinedGroupAsync(string memberId, int groupStartYearMonth, GroupEntity group)
    {
        // The join is already done in GroupRepository. Just return the current state.
        return await GetMemberGroupAsync(memberId, groupStartYearMonth) ?? new MemberGroupEntity { MemberId = memberId, GroupStartYearMonth = groupStartYearMonth };
    }

    public async Task<MemberGroupEntity> LeaveGroupAsync(string memberId, int groupStartYearMonth, string groupId)
    {
        // The leave is already done in GroupRepository. Just return the current state.
        return await GetMemberGroupAsync(memberId, groupStartYearMonth) ?? new MemberGroupEntity { MemberId = memberId, GroupStartYearMonth = groupStartYearMonth };
    }

    public async Task<MemberGroupEntity> CloseCreatedGroup(string memberId, int groupStartYearMonth, string groupId)
    {
        // The close is already done in GroupRepository. Just return the current state.
        return await GetMemberGroupAsync(memberId, groupStartYearMonth) ?? new MemberGroupEntity { MemberId = memberId, GroupStartYearMonth = groupStartYearMonth };
    }

    public Task CloseAllJoinedGroup(HashSet<string> memberIds, int groupStartYearMonth, string groupId)
    {
        // Redundant in relational model if Groups table is updated.
        return Task.CompletedTask;
    }

    public Task UpdateAllJoinedGroup(HashSet<string> memberIds, int oldGroupStartYearMonth, int newGroupStartYearMonth, GroupEntity group)
    {
        // Redundant in relational model if Groups table is updated.
        return Task.CompletedTask;
    }
}

