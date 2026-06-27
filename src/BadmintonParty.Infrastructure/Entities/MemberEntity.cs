namespace BadmintonParty.Infrastructure.Entities;

public class MemberEntity
{
    public string MemberId { get; set; } = string.Empty;

    public string LineUserId { get; set; } = string.Empty;

    public string MemberName { get; set; } = string.Empty;

    public string PictureUrl { get; set; } = string.Empty;

    public DateTime CreateTime { get; set; }

    public DateTime UpdateTime { get; set; }
}
