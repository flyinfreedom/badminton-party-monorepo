namespace BadmiintonParty.Liff.Web.Api.Repositories;

using Amazon.DynamoDBv2.DataModel;
using BadmiintonParty.Liff.Web.Api.Contexts;
using BadmiintonParty.Liff.Web.Api.Entities;

public class CourtRepository : ICourtRepository
{
    private readonly DynamoDBContext _dbContext;

    public CourtRepository(IDynamoContext context)
    {
        _dbContext = context.DbContext;
    }

    public Task<CourtEntity?> GetCourtById(string courtId)
        => _dbContext.LoadAsync<CourtEntity?>(courtId);

    public Task<List<CourtEntity>> GetCourtsAsync()
        => _dbContext.ScanAsync<CourtEntity>(new List<ScanCondition>()).GetRemainingAsync();
}
