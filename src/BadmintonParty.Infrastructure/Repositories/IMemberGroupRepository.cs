using BadmintonParty.Infrastructure.Entities;

namespace BadmintonParty.Infrastructure.Repositories;

public interface IMemberGroupRepository
{
    public Task<List<MemberGroupEntity>> GetCurrentMemberGroupAsync(string memberId, int currentYearMonth);
    public Task<MemberGroupEntity?> GetMemberGroupAsync(string memberId, int currentYearMonth);
    public Task<MemberGroupEntity> CreateGroupAsync(string memberId, int groupStartYearMonth, GroupEntity group);
    public Task<MemberGroupEntity> JoinedGroupAsync(string memberId, int groupStartYearMonth, GroupEntity group);
    public Task<MemberGroupEntity> LeaveGroupAsync(string memberId, int groupStartYearMonth, string groupId);
    public Task<MemberGroupEntity> CloseCreatedGroup(string memberId, int groupStartYearMonth, string groupId);
    public Task CloseAllJoinedGroup(HashSet<string> memberIds, int groupStartYearMonth, string groupId);
    public Task UpdateAllJoinedGroup(HashSet<string> memberIds, int oldGroupStartYearMonth, int newGroupStartYearMonth, GroupEntity group);
}
