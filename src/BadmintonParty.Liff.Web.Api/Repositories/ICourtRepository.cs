namespace BadmintonParty.Liff.Web.Api.Repositories;

using BadmintonParty.Liff.Web.Api.Entities;

public interface ICourtRepository
{
    public Task<CourtEntity?> GetCourtById(string courtId);
    public Task<List<CourtEntity>> GetCourtsAsync();
}

