namespace BadmiintonParty.Liff.Web.Api.Repositories;

using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using BadmiintonParty.Liff.Web.Api.Contexts;
using BadmiintonParty.Liff.Web.Api.Entities;
using BadmiintonParty.Liff.Web.Api.Enums;

public class MemberGroupRepository : IMemberGroupRepository
{
    private readonly DynamoDBContext _dbContext;

    public MemberGroupRepository(IDynamoContext context)
    {
        _dbContext = context.DbContext;
    }

    public async Task<List<MemberGroupEntity>> GetCurrentMemberGroupAsync(string memberId, int currentYearMonth)
    {
        var query = await _dbContext.QueryAsync<MemberGroupEntity>(memberId, new DynamoDBOperationConfig
        {
            QueryFilter = [new ScanCondition("GroupStartYearMonth", ScanOperator.GreaterThanOrEqual, currentYearMonth)]
        }).GetRemainingAsync();
        return query ?? [];
    }

    public Task<MemberGroupEntity?> GetMemberGroupAsync(string memberId, int currentYearMonth)
        => _dbContext.LoadAsync<MemberGroupEntity?>(memberId, currentYearMonth);

    public async Task<MemberGroupEntity> CreateGroupAsync(string memberId, int groupStartYearMonth, GroupEntity group)
    {
        var memberGroup = await GetOrCreateMemberGroupEntityAsync(memberId, groupStartYearMonth);
        if (memberGroup.CreatedGroups.Any(g => g.GroupId == group.GroupId)) return memberGroup;

        memberGroup.CreatedGroups.Add(new GroupInfo
        {
            GroupId = group.GroupId,
            StartTime = group.StartTime,
            EndTime = group.EndTime,
            GroupStatus = group.GroupStatus
        });

        await _dbContext.SaveAsync(memberGroup);
        return memberGroup;
    }

    public async Task<MemberGroupEntity> JoinedGroupAsync(string memberId, int groupStartYearMonth, GroupEntity group)
    {
        var memberGroup = await GetOrCreateMemberGroupEntityAsync(memberId, groupStartYearMonth);
        if (memberGroup.JoinedGroups.Any(g => g.GroupId == group.GroupId)) return memberGroup;

        memberGroup.JoinedGroups.Add(new GroupInfo
        {
            GroupId = group.GroupId,
            StartTime = group.StartTime,
            EndTime = group.EndTime,
            GroupStatus = group.GroupStatus
        });

        await _dbContext.SaveAsync(memberGroup);
        return memberGroup;
    }

    public async Task<MemberGroupEntity> LeaveGroupAsync(string memberId, int groupStartYearMonth, string groupId)
    {
        var memberGroup = await GetOrCreateMemberGroupEntityAsync(memberId, groupStartYearMonth);
        memberGroup.JoinedGroups.RemoveAll(g => g.GroupId == groupId);

        await _dbContext.SaveAsync(memberGroup);
        return memberGroup;
    }

    public async Task<MemberGroupEntity> CloseCreatedGroup(string memberId, int groupStartYearMonth, string groupId)
    {
        var memberGroup = await GetOrCreateMemberGroupEntityAsync(memberId, groupStartYearMonth);
        var group = memberGroup.CreatedGroups.FirstOrDefault(g => g.GroupId == groupId);
        if (group is null) return memberGroup;

        group.GroupStatus = GroupStatus.Closed;
        await _dbContext.SaveAsync(memberGroup);
        return memberGroup;
    }

    public async Task UpdateAllJoinedGroup(HashSet<string> memberIds, int oldGroupStartYearMonth, int newGroupStartYearMonth, GroupEntity group)
    {
        var tasks = memberIds.Select(memberId => UpdateJoinedGroup(memberId, oldGroupStartYearMonth, newGroupStartYearMonth, group));
        await Task.WhenAll(tasks);
    }

    private async Task UpdateJoinedGroup(string memberId, int oldGroupStartYearMonth, int newGroupStartYearMonth, GroupEntity group)
    {
        var memberGroup = await GetOrCreateMemberGroupEntityAsync(memberId, oldGroupStartYearMonth);
        memberGroup.JoinedGroups.RemoveAll(record => record.GroupId == group.GroupId);
        await _dbContext.SaveAsync(memberGroup);

        var newRecord = new GroupInfo
        {
            GroupId = group.GroupId,
            StartTime = group.StartTime,
            EndTime = group.EndTime,
            GroupStatus = group.GroupStatus
        };

        if (oldGroupStartYearMonth != newGroupStartYearMonth)
        {
            var newMemberGroup = await GetOrCreateMemberGroupEntityAsync(memberId, newGroupStartYearMonth);
            newMemberGroup.JoinedGroups.Add(newRecord);
            await _dbContext.SaveAsync(newMemberGroup);
        }
        else
        {
            memberGroup.JoinedGroups.Add(newRecord);
            await _dbContext.SaveAsync(memberGroup);
        }
    }

    public async Task CloseAllJoinedGroup(HashSet<string> memberIds, int groupStartYearMonth, string groupId)
    {
        var tasks = memberIds.Select(memberId => CloseJoinedGroup(memberId, groupStartYearMonth, groupId));
        await Task.WhenAll(tasks);
    }

    private async Task CloseJoinedGroup(string memberId, int groupStartYearMonth, string groupId)
    {
        var memberGroup = await GetOrCreateMemberGroupEntityAsync(memberId, groupStartYearMonth);
        var group = memberGroup.JoinedGroups.FirstOrDefault(g => g.GroupId == groupId);
        if (group is null) return;

        group.GroupStatus = GroupStatus.Closed;
        await _dbContext.SaveAsync(memberGroup);
    }

    private async Task<MemberGroupEntity> GetOrCreateMemberGroupEntityAsync(string memberId, int groupStartYearMonth)
    {
        var memberGroup = await _dbContext.LoadAsync<MemberGroupEntity?>(memberId, groupStartYearMonth);
        return memberGroup ?? new MemberGroupEntity
        {
            MemberId = memberId,
            GroupStartYearMonth = groupStartYearMonth,
            CreatedGroups = [],
            JoinedGroups = []
        };
    }
}
