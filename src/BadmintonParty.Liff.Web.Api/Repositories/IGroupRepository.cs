namespace BadmintonParty.Liff.Web.Api.Repositories;

using BadmintonParty.Liff.Web.Api.Entities;
using BadmintonParty.Liff.Web.Api.Models;

public interface IGroupRepository
{
    public Task<GroupEntity> GetGroupById(string groupId);
    public Task<List<GroupEntity>> GetGroupsByCourtId(string courtId);
    public Task<List<GroupEntity>> GetGroupByBatchGetItemAsync(HashSet<string> groupIds);
    public Task<bool> JoinGroup(string groupId, GroupMember groupMember);
    public Task<bool> MinusOne(string groupId, string memberId);
    public Task<bool> LeaveGroup(string groupId, string memberId);
    public Task<GroupEntity> CreateGroup(GroupEntity entity);
    public Task<bool> UpdateGroup(GroupEntity entity);
    public Task<bool> CloseGroup(string groupId);
}

