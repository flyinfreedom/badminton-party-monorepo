namespace BadmiintonParty.Liff.Web.Api.Contexts;

using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

public interface IDynamoContext : IDisposable
{
    public DynamoDBContext DbContext { get; }
    public AmazonDynamoDBClient Client { get; }
    public Table Courts { get; }
    public Table Groups { get; }
    public Table Members { get; }
    public Table MemberGroups { get; }
}
