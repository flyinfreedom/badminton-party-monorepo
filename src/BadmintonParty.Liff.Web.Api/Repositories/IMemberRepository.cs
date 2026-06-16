namespace BadmintonParty.Liff.Web.Api.Repositories;

using BadmintonParty.Liff.Web.Api.Entities;

public interface IMemberRepository
{
    public Task<string> GetMemberIdByLineId(string lineId);
    public Task<string> CreateMember(string lineUserId, string userName, string pictureUrl);
    public Task<bool> UpdateMember(MemberEntity entity);
    public Task<MemberEntity> GetMember(string memberId);
    public IEnumerable<MemberEntity> GetMembers(HashSet<string> memberIds);
    public Task<List<MemberRecentOpeningEntity>> GetRecentOpenings(string memberId);
    public Task<bool> UpdateRecentOpening(string memberId, string courtId);
}

