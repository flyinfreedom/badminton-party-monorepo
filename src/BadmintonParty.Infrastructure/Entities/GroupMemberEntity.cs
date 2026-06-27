namespace BadmintonParty.Infrastructure.Entities;

public class GroupMemberEntity
{
    public string GroupMemberId { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public DateTime JoinTime { get; set; }

    // Navigation properties
    public GroupEntity Group { get; set; } = null!;
    public MemberEntity Member { get; set; } = null!;
}
