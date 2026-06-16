namespace BadmintonParty.Liff.Web.Api.Models;

public class GroupMember
{
    public string MemberId { get; set; } = string.Empty;
    public string LineUserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PictureUrl { get; set; } = string.Empty;
    public DateTime JoinTime { get; set; }
}

