using BadmintonParty.Infrastructure.Enums;
using BadmintonParty.Infrastructure.Contexts;
using BadmintonParty.Infrastructure.Extensions;
using BadmintonParty.Infrastructure.Models;
namespace BadmintonParty.Liff.Web.Api.Services;

using BadmintonParty.Infrastructure.Entities;
using BadmintonParty.Infrastructure.Repositories;

public class CourtService
{
    private readonly ICourtRepository _courtRepository;
    private readonly IGroupRepository _groupRepository;

    public CourtService(ICourtRepository courtRepository, IGroupRepository groupRepository)
    {
        _courtRepository = courtRepository;
        _groupRepository = groupRepository;
    }

    public Task<List<CourtEntity>> GetCourtsAsync() => _courtRepository.GetCourtsAsync();

    public Task<CourtEntity?> GetCourtById(string courtId) => _courtRepository.GetCourtById(courtId);

    public Task<List<GroupEntity>> GetGroupsByCourtId(string courtId) => _groupRepository.GetGroupsByCourtId(courtId);
}

