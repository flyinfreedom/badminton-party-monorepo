namespace BadmiintonParty.Liff.Web.Api.Repositories;

using BadmiintonParty.Liff.Web.Api.Entities;

public interface IMemberRepository
{
    public Task<string> GetMemberIdByLineId(string lineId);
    public Task<string> CreateMember(string lineUserId, string userName, string pictureUrl);
    public Task<bool> UpdateMember(MemberEntity entity);
    public Task<MemberEntity> GetMember(string memberId);
    public IEnumerable<MemberEntity> GetMembers(HashSet<string> memberIds);
}
