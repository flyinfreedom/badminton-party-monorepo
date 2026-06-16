namespace BadmintonParty.Liff.Web.Api.Services;

using BadmintonParty.Liff.Web.Api.Entities;
using BadmintonParty.Liff.Web.Api.Models;
using BadmintonParty.Liff.Web.Api.Repositories;

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
        return await _memberRepository.UpdateRecentOpening(memberId, request.CourtId);
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

