namespace BadmiintonParty.Liff.Web.Api.Entities;

using Amazon.DynamoDBv2.DataModel;

[DynamoDBTable("Members")]
public class MemberEntity
{
    [DynamoDBHashKey]
    public string MemberId { get; set; } = string.Empty;

    [DynamoDBGlobalSecondaryIndexHashKey("LineUserIdIndex")]
    public string LineUserId { get; set; } = string.Empty;

    public string MemberName { get; set; } = string.Empty;

    public string PictureUrl { get; set; } = string.Empty;

    public List<RecentOpening> RecentOpenings { get; set; } = [];

    public DateTime CreateTime { get; set; }

    public DateTime UpdateTime { get; set; }
}

public class RecentOpening
{
    public string CourtId { get; set; } = string.Empty;

    public string CourtName { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public DateTime OpeningTime { get; set; }
}
