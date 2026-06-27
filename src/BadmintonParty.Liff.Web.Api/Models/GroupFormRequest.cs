using BadmintonParty.Infrastructure.Entities;
using BadmintonParty.Infrastructure.Repositories;
using BadmintonParty.Infrastructure.Contexts;
using BadmintonParty.Infrastructure.Extensions;
using BadmintonParty.Infrastructure.Models;
namespace BadmintonParty.Liff.Web.Api.Models;

using BadmintonParty.Infrastructure.Enums;

public class GroupFormRequest
{
    public string GroupName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int PlayTime { get; set; }
    public string? CourtId { get; set; }
    public string CourtName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public ConsumptionPatterns ConsumptionPatterns { get; set; }
    public int Amount { get; set; }
    public int MinPeople { get; set; }
    public int MaxPeople { get; set; }
    public int AlternatePeople { get; set; }
    public LevelGroup LevelGroup { get; set; }
    public bool IsPrivate { get; set; }
    public string OtherInfo { get; set; } = string.Empty;
}

