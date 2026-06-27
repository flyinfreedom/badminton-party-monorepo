using BadmintonParty.Infrastructure.Entities;
using BadmintonParty.Infrastructure.Repositories;
using BadmintonParty.Infrastructure.Enums;
using BadmintonParty.Infrastructure.Contexts;
using BadmintonParty.Infrastructure.Extensions;
using BadmintonParty.Infrastructure.Models;
namespace BadmintonParty.Liff.Web.Api.Models;

public class GetMemberProfileRequest
{
    public string LineUserId { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string PictureUrl { get; set; } = string.Empty;
}

