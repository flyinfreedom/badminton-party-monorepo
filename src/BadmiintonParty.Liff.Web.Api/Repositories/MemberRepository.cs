namespace BadmiintonParty.Liff.Web.Api.Repositories;

using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using BadmiintonParty.Liff.Web.Api.Contexts;
using BadmiintonParty.Liff.Web.Api.Entities;
using BadmiintonParty.Liff.Web.Api.Exceptions;

public class MemberRepository : IMemberRepository
{
    private readonly IDynamoContext _context;
    private readonly DynamoDBContext _dbContext;

    public MemberRepository(IDynamoContext context)
    {
        _context = context;
        _dbContext = _context.DbContext;
    }

    public async Task<string> CreateMember(string lineUserId, string userName, string pictureUrl)
    {
        if (!string.IsNullOrEmpty(await GetMemberIdByLineId(lineUserId)))
        {
            throw new CustomException($"line id: {lineUserId} has existed");
        }

        var newMemberId = Guid.NewGuid().ToString();
        var newMember = new MemberEntity
        {
            MemberId = newMemberId,
            LineUserId = lineUserId,
            MemberName = userName,
            PictureUrl = pictureUrl,
            RecentOpenings = [],
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };

        await _dbContext.SaveAsync(newMember);
        return newMemberId;
    }

    public async Task<string> GetMemberIdByLineId(string lineUserId)
    {
        var filter = new QueryFilter(nameof(MemberEntity.LineUserId), QueryOperator.Equal, lineUserId);
        var queryConfig = new QueryOperationConfig
        {
            IndexName = "LineUserIdIndex",
            Filter = filter
        };

        var search = _context.Members.Query(queryConfig);
        var results = await search.GetNextSetAsync();

        if (results is null || results.Count == 0) return string.Empty;

        return results.First()["MemberId"].AsString();
    }

    public async Task<bool> UpdateMember(MemberEntity entity)
    {
        await _dbContext.SaveAsync(entity);
        return true;
    }

    public async Task<MemberEntity> GetMember(string memberId)
    {
        var member = await _dbContext.LoadAsync<MemberEntity>(memberId);
        if (member is null) throw new CustomException($"找不到MemberId為 [{memberId}] 的使用者");
        return member;
    }

    public IEnumerable<MemberEntity> GetMembers(HashSet<string> memberIds)
    {
        if (memberIds.Count == 0) yield break;

        var request = new BatchGetItemRequest
        {
            RequestItems = new Dictionary<string, KeysAndAttributes>
            {
                {
                    "Members",
                    new KeysAndAttributes
                    {
                        Keys = memberIds.Select(key => new Dictionary<string, AttributeValue>
                        {
                            { "MemberId", new AttributeValue(key) }
                        }).ToList()
                    }
                }
            }
        };

        var batchResult = _context.Client.BatchGetItemAsync(request).GetAwaiter().GetResult();

        foreach (var item in batchResult.Responses["Members"])
        {
            var document = Document.FromAttributeMap(item);
            yield return new MemberEntity
            {
                MemberId = document["MemberId"].AsString(),
                MemberName = document["MemberName"].AsString(),
                PictureUrl = document["PictureUrl"].AsString(),
                LineUserId = document["LineUserId"].AsString(),
            };
        }
    }
}
