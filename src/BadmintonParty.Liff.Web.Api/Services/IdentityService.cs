using BadmintonParty.Infrastructure.Entities;
using BadmintonParty.Infrastructure.Enums;
using BadmintonParty.Infrastructure.Contexts;
using BadmintonParty.Infrastructure.Extensions;
namespace BadmintonParty.Liff.Web.Api.Services;

using BadmintonParty.Liff.Web.Api.Models;
using BadmintonParty.Infrastructure.Models;
using BadmintonParty.Infrastructure.Repositories;
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
        var courtRepo = scope.ServiceProvider.GetRequiredService<ICourtRepository>();

        var memberId = await repo.GetMemberIdByLineId(request.LineUserId);
        if (string.IsNullOrEmpty(memberId))
        {
            memberId = await repo.CreateMember(request.LineUserId, request.MemberName, request.PictureUrl);
        }

        var member = await repo.GetMember(memberId);
        var recentOpeningsEntities = await repo.GetRecentOpenings(memberId);
        
        var recentOpenings = new List<RecentOpening>();
        foreach (var ro in recentOpeningsEntities)
        {
            var court = await courtRepo.GetCourtById(ro.CourtId);
            if (court != null)
            {
                recentOpenings.Add(new RecentOpening
                {
                    CourtId = court.CourtId,
                    CourtName = court.CourtName,
                    Location = court.Location,
                    OpeningTime = ro.OpeningTime
                });
            }
        }

        return new MemberProfile
        {
            MemberId = member.MemberId,
            LineUserId = member.LineUserId,
            PictureUrl = member.PictureUrl,
            DisplayName = member.MemberName,
            RecentOpenings = recentOpenings
        };
    }

    public MemberProfile SetMemberProfileToCache(string token, MemberProfile profile, TimeSpan expiration)
    {
        _memoryCache.Remove(token);
        return _memoryCache.Set(token, profile, expiration);
    }

    public MemberProfile? GetMemberProfileFromCache(string token) => _memoryCache.Get<MemberProfile>(token);
}

