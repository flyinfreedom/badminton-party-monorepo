namespace BadmiintonParty.Liff.Web.Api.Entities;

using Amazon.DynamoDBv2.DataModel;
using BadmiintonParty.Liff.Web.Api.Enums;

[DynamoDBTable("Groups")]
public class GroupEntity
{
    [DynamoDBHashKey]
    public string GroupId { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public GroupStatus GroupStatus { get; set; }
    public string Avatar { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int PlayTime { get; set; }
    public string CourtId { get; set; } = string.Empty;
    public string CourtName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public ConsumptionPatterns ConsumptionPatterns { get; set; }
    public int Amount { get; set; }
    public int MinPeople { get; set; }
    public int MaxPeople { get; set; }
    public int AlternatePeople { get; set; }
    public LevelGroup LevelGroup { get; set; }
    public string OtherInfo { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime UpdateTime { get; set; }
    public List<GroupMember> JoinedMembers { get; set; } = [];
}

public class GroupMember
{
    public string MemberId { get; set; } = string.Empty;
    public string LineUserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PictureUrl { get; set; } = string.Empty;
    public DateTime JoinTime { get; set; }
}
