namespace BadmintonParty.Liff.Web.Api.Models;

public class MemberProfile
{
    public string MemberId { get; set; } = string.Empty;
    public string LineUserId { get; set; } = string.Empty;
    public string PictureUrl { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<RecentOpening> RecentOpenings { get; set; } = [];
}

public class RecentOpening
{
    public string CourtId { get; set; } = string.Empty;
    public string CourtName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime OpeningTime { get; set; }
}

