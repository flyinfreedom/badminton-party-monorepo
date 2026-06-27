using BadmintonParty.Infrastructure.Contexts;
using BadmintonParty.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace BadmintonParty.Infrastructure.Repositories;

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
