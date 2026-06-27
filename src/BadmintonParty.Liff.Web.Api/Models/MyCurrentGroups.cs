using BadmintonParty.Infrastructure.Entities;
using BadmintonParty.Infrastructure.Repositories;
using BadmintonParty.Infrastructure.Enums;
using BadmintonParty.Infrastructure.Contexts;
using BadmintonParty.Infrastructure.Extensions;
using BadmintonParty.Infrastructure.Models;
namespace BadmintonParty.Liff.Web.Api.Models;

public class MyCurrentGroups
{
    public List<GroupResponse> CreatedGroups { get; set; } = [];
    public List<GroupResponse> JoinedGroups { get; set; } = [];
}

