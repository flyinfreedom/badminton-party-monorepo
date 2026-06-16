namespace BadmiintonParty.Liff.Web.Api.Contexts;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

public class DynamoContext : IDynamoContext
{
    private readonly AmazonDynamoDBClient _client;
    private readonly DynamoDBContext _context;

    private readonly Table _courts;
    private readonly Table _groups;
    private readonly Table _members;
    private readonly Table _memberGroups;

    public Table Courts => _courts;
    public Table Groups => _groups;
    public Table Members => _members;
    public Table MemberGroups => _memberGroups;

    public DynamoContext(IConfiguration configuration)
    {
        var serviceUrl = configuration["DynamoDB:ServiceURL"];
        if (!string.IsNullOrEmpty(serviceUrl))
        {
            var config = new AmazonDynamoDBConfig { ServiceURL = serviceUrl };
            _client = new AmazonDynamoDBClient(config);
        }
        else
        {
            _client = new AmazonDynamoDBClient();
        }

        _context = new DynamoDBContext(_client);
        _courts = (Table)Table.LoadTable(_client, "Courts");
        _groups = (Table)Table.LoadTable(_client, "Groups");
        _members = (Table)Table.LoadTable(_client, "Members");
        _memberGroups = (Table)Table.LoadTable(_client, "MemberGroups");
    }

    public DynamoDBContext DbContext => _context;
    public AmazonDynamoDBClient Client => _client;

    public void Dispose()
    {
        _client.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
