using BadmintonParty.Infrastructure.Entities;
using BadmintonParty.Infrastructure.Repositories;
using BadmintonParty.Infrastructure.Enums;
using BadmintonParty.Infrastructure.Contexts;
using BadmintonParty.Infrastructure.Extensions;
using BadmintonParty.Infrastructure.Models;
namespace BadmintonParty.Liff.Web.Api.Models;

public class MemberInitResponse
{
    public MemberProfile Profile { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
}
