namespace BadmiintonParty.Liff.Web.Api.Services;

using BadmiintonParty.Liff.Web.Api.Models;
using BadmiintonParty.Liff.Web.Api.Repositories;
using Microsoft.Extensions.Caching.Memory;

public class IdentityService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _memoryCache;

    public IdentityService(IServiceProvider serviceProvider, IMemoryCache memoryCache)
    {
        _serviceProvider = serviceProvider;
        _memoryCache = memoryCache;
    }

    public async Task<MemberProfile> GetMemberProfile(GetMemberProfileRequest request)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IMemberRepository>();

        var memberId = await repo.GetMemberIdByLineId(request.LineUserId);
        if (string.IsNullOrEmpty(memberId))
        {
            memberId = await repo.CreateMember(request.LineUserId, request.MemberName, request.PictureUrl);
        }

        var member = await repo.GetMember(memberId);
        return new MemberProfile
        {
            MemberId = member.MemberId,
            LineUserId = member.LineUserId,
            PictureUrl = member.PictureUrl,
            DisplayName = member.MemberName,
            RecentOpenings = member.RecentOpenings
        };
    }

    public MemberProfile SetMemberProfileToCache(string token, MemberProfile profile, TimeSpan expiration)
    {
        _memoryCache.Remove(token);
        return _memoryCache.Set(token, profile, expiration);
    }

    public MemberProfile? GetMemberProfileFromCache(string token) => _memoryCache.Get<MemberProfile>(token);
}
