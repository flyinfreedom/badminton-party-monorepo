namespace BadmiintonParty.Liff.Web.Api.Repositories;

using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using BadmiintonParty.Liff.Web.Api.Contexts;
using BadmiintonParty.Liff.Web.Api.Entities;
using BadmiintonParty.Liff.Web.Api.Enums;
using BadmiintonParty.Liff.Web.Api.Exceptions;
using BadmiintonParty.Liff.Web.Api.Extensions;

public class GroupRepository : IGroupRepository
{
    private readonly IDynamoContext _context;
    private readonly DynamoDBContext _dbContext;
    private readonly IMemberGroupRepository _memberGroupRepository;

    public GroupRepository(IDynamoContext context, IMemberGroupRepository memberGroupRepository)
    {
        _context = context;
        _dbContext = _context.DbContext;
        _memberGroupRepository = memberGroupRepository;
    }

    public async Task<bool> CloseGroup(string groupId)
    {
        var group = await _dbContext.LoadAsync<GroupEntity>(groupId);
        if (group is null) return false;

        group.GroupStatus = GroupStatus.Closed;
        await _dbContext.SaveAsync(group);
        return true;
    }

    public async Task<GroupEntity> CreateGroup(GroupEntity entity)
    {
        await _dbContext.SaveAsync(entity);
        return entity;
    }

    public async Task<GroupEntity> GetGroupById(string groupId)
    {
        var group = await _dbContext.LoadAsync<GroupEntity>(groupId);
        if (group is null) throw new CustomException("查無此羽球團");
        return group;
    }

    public Task<List<GroupEntity>> GetGroupsByCourtId(string courtId)
        => _dbContext.ScanAsync<GroupEntity>(new List<ScanCondition>
        {
            new("CourtId", ScanOperator.Equal, courtId),
            new("IsPrivate", ScanOperator.Equal, false),
            new("GroupStatus", ScanOperator.Equal, GroupStatus.Opened),
            new("EndTime", ScanOperator.GreaterThan, DateTime.UtcNow)
        }).GetNextSetAsync();

    public IEnumerable<GroupEntity> GetGroupByBatchGetItem(HashSet<string> groupIds)
    {
        if (groupIds.Count == 0) yield break;

        var request = new BatchGetItemRequest
        {
            RequestItems = new Dictionary<string, KeysAndAttributes>
            {
                {
                    "Groups",
                    new KeysAndAttributes
                    {
                        Keys = groupIds.Select(key => new Dictionary<string, AttributeValue>
                        {
                            { "GroupId", new AttributeValue(key) }
                        }).ToList()
                    }
                }
            }
        };

        var batchResult = _context.Client.BatchGetItemAsync(request).GetAwaiter().GetResult();

        foreach (var item in batchResult.Responses["Groups"])
        {
            var document = Document.FromAttributeMap(item);
            yield return new GroupEntity
            {
                GroupId = document["GroupId"].AsString(),
                GroupName = document["GroupName"].AsString(),
                StartTime = DateTime.Parse(document["StartTime"].AsString()),
                GroupStatus = (GroupStatus)document["GroupStatus"].AsInt(),
                EndTime = DateTime.Parse(document["EndTime"].AsString()),
                PlayTime = document["PlayTime"].AsInt(),
                CourtName = document["CourtName"].AsString(),
                ConsumptionPatterns = (ConsumptionPatterns)document["ConsumptionPatterns"].AsInt(),
                Amount = document["Amount"].AsInt(),
                LevelGroup = (LevelGroup)document["LevelGroup"].AsInt(),
                MinPeople = document["MinPeople"].AsInt(),
                MaxPeople = document["MaxPeople"].AsInt(),
                AlternatePeople = document["AlternatePeople"].AsInt(),
                Avatar = document["Avatar"].AsString(),
                JoinedMembers = document["JoinedMembers"].AsListOfDocument().Select(member => new GroupMember
                {
                    MemberId = member["MemberId"].AsString()
                }).ToList()
            };
        }
    }

    public async Task<bool> JoinGroup(string groupId, GroupMember groupMember)
    {
        var group = await _dbContext.LoadAsync<GroupEntity>(groupId);
        if (group == null) return false;

        group.JoinedMembers.Add(groupMember);
        await _dbContext.SaveAsync(group);
        if (group.MemberId != groupMember.MemberId)
        {
            await _memberGroupRepository.JoinedGroupAsync(groupMember.MemberId, group.StartTime.ToYearMonthInteger(), group);
        }
        return true;
    }

    public async Task<bool> MinusOne(string groupId, string memberId)
    {
        var group = await _dbContext.LoadAsync<GroupEntity>(groupId);
        if (group == null) return false;

        var index = group.JoinedMembers.FindLastIndex(member => member.MemberId == memberId);
        if (index >= 0)
        {
            group.JoinedMembers.RemoveAt(index);
            await _dbContext.SaveAsync(group);
            if (group.JoinedMembers.All(member => memberId != member.MemberId))
            {
                await _memberGroupRepository.LeaveGroupAsync(memberId, group.StartTime.ToYearMonthInteger(), group.GroupId);
            }
        }
        return true;
    }

    public async Task<bool> LeaveGroup(string groupId, string memberId)
    {
        var group = await _dbContext.LoadAsync<GroupEntity>(groupId);
        if (group == null) return false;

        group.JoinedMembers.RemoveAll(member => member.MemberId == memberId);
        await _dbContext.SaveAsync(group);
        await _memberGroupRepository.LeaveGroupAsync(memberId, group.StartTime.ToYearMonthInteger(), group.GroupId);
        return true;
    }

    public async Task<bool> UpdateGroup(GroupEntity entity)
    {
        var group = await _dbContext.LoadAsync<GroupEntity>(entity.GroupId);
        if (group == null) return false;

        await _dbContext.SaveAsync(entity);
        return true;
    }
}
