namespace BadmiintonParty.Liff.Web.Api.Models;

using BadmiintonParty.Liff.Web.Api.Entities;

public class MemberProfile
{
    public string MemberId { get; set; } = string.Empty;
    public string LineUserId { get; set; } = string.Empty;
    public string PictureUrl { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<RecentOpening> RecentOpenings { get; set; } = [];
}
