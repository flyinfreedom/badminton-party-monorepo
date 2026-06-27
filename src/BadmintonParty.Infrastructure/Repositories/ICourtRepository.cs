using BadmintonParty.Infrastructure.Entities;

namespace BadmintonParty.Infrastructure.Repositories;

public interface ICourtRepository
{
    public Task<CourtEntity?> GetCourtById(string courtId);
    public Task<List<CourtEntity>> GetCourtsAsync();
}
