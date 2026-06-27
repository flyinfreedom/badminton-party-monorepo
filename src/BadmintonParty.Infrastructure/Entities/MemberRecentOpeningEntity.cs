namespace BadmintonParty.Infrastructure.Entities;

public class MemberRecentOpeningEntity
{
    public string MemberId { get; set; } = string.Empty;
    public string CourtId { get; set; } = string.Empty;
    public DateTime OpeningTime { get; set; }
}
