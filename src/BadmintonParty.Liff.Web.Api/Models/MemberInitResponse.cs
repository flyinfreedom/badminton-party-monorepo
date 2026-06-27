namespace BadmintonParty.Liff.Web.Api.Models;

public class MemberInitResponse
{
    public MemberProfile Profile { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
}
