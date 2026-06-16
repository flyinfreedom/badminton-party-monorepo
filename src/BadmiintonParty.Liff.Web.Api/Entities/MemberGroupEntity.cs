namespace BadmiintonParty.Liff.Web.Api.Entities;

using Amazon.DynamoDBv2.DataModel;
using BadmiintonParty.Liff.Web.Api.Enums;

[DynamoDBTable("MemberGroups")]
public class MemberGroupEntity
{
    [DynamoDBHashKey]
    public string MemberId { get; set; } = string.Empty;
    [DynamoDBRangeKey]
    public int GroupStartYearMonth { get; set; }
    public List<GroupInfo> CreatedGroups { get; set; } = [];
    public List<GroupInfo> JoinedGroups { get; set; } = [];
}

public class GroupInfo
{
    public string GroupId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public GroupStatus GroupStatus { get; set; }
}
