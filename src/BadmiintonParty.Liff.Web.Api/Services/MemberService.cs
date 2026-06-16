namespace BadmiintonParty.Liff.Web.Api.Services;

using BadmiintonParty.Liff.Web.Api.Entities;
using BadmiintonParty.Liff.Web.Api.Models;
using BadmiintonParty.Liff.Web.Api.Repositories;

public class MemberService
{
    private readonly IMemberRepository _memberRepository;

    public MemberService(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public Task<MemberEntity> GetMember(string memberId) => _memberRepository.GetMember(memberId);

    public IEnumerable<MemberEntity> GetMembers(HashSet<string> memberIds) => _memberRepository.GetMembers(memberIds);

    public async Task<bool> HandleRecentOpening(string memberId, GroupFormRequest request)
    {
        const int maxRecentCount = 3;
        var member = await _memberRepository.GetMember(memberId);
        var recent = member.RecentOpenings.OrderByDescending(ro => ro.OpeningTime).ToList();

        var existing = recent.FirstOrDefault(r => r.CourtId == request.CourtId);
        if (existing != null)
        {
            existing.OpeningTime = DateTime.UtcNow;
            member.RecentOpenings = recent;
            return await _memberRepository.UpdateMember(member);
        }

        if (recent.Count >= maxRecentCount)
        {
            recent.RemoveRange(maxRecentCount - 1, recent.Count - maxRecentCount + 1);
        }

        recent.Add(new RecentOpening
        {
            CourtId = request.CourtId,
            CourtName = request.CourtName,
            Location = request.Location,
            OpeningTime = DateTime.UtcNow
        });

        member.RecentOpenings = recent;
        return await _memberRepository.UpdateMember(member);
    }

    public async Task<bool> UpdateUserName(string memberId, string userName)
    {
        var member = await _memberRepository.GetMember(memberId);
        member.MemberName = userName;
        return await _memberRepository.UpdateMember(member);
    }

    public async Task<bool> UpdateUserAvatar(string memberId, string avatarUrl)
    {
        var member = await _memberRepository.GetMember(memberId);
        member.PictureUrl = avatarUrl;
        return await _memberRepository.UpdateMember(member);
    }
}
