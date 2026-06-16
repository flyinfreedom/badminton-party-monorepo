namespace BadmintonParty.Liff.Web.Api.Repositories;

using BadmintonParty.Liff.Web.Api.Contexts;
using BadmintonParty.Liff.Web.Api.Entities;
using Microsoft.EntityFrameworkCore;

public class CourtRepository : ICourtRepository
{
    private readonly BadmintonContext _dbContext;

    public CourtRepository(BadmintonContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CourtEntity?> GetCourtById(string courtId)
        => await _dbContext.Courts.FindAsync(courtId);

    public Task<List<CourtEntity>> GetCourtsAsync()
        => _dbContext.Courts.ToListAsync();
}

