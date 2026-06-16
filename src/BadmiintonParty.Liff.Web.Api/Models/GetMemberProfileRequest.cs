namespace BadmiintonParty.Liff.Web.Api.Models;

public class GetMemberProfileRequest
{
    public string LineUserId { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string PictureUrl { get; set; } = string.Empty;
}
