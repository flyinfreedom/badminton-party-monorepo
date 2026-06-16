namespace BadmintonParty.Liff.Web.Api.Entities;

using BadmintonParty.Liff.Web.Api.Enums;

public class MemberGroupEntity
{
    public string MemberId { get; set; } = string.Empty;
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

