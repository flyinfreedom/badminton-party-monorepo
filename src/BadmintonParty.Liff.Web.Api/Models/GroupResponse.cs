namespace BadmintonParty.Liff.Web.Api.Models;

using BadmintonParty.Liff.Web.Api.Entities;
using BadmintonParty.Liff.Web.Api.Enums;

public class GroupResponse
{
    public string GroupId { get; set; }
    public string GroupName { get; set; }
    public GroupStatus GroupStatus { get; set; }
    public string Avatar { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int PlayTime { get; set; }
    public string CourtId { get; set; }
    public string CourtName { get; set; }
    public string Location { get; set; }
    public ConsumptionPatterns ConsumptionPatterns { get; set; }
    public int Amount { get; set; }
    public int MinPeople { get; set; }
    public int MaxPeople { get; set; }
    public int AlternatePeople { get; set; }
    public LevelGroup LevelGroup { get; set; }
    public string OtherInfo { get; set; }
    public string MemberId { get; set; }
    public string MemberName { get; set; }
    public bool IsPrivate { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime UpdateTime { get; set; }
    public List<GroupMember> JoinedMembers { get; set; }

    public GroupResponse(GroupEntity entity, string memberName = "")
    {
        GroupId = entity.GroupId;
        GroupName = entity.GroupName;
        GroupStatus = entity.GroupStatus;
        Avatar = entity.Avatar;
        StartTime = entity.StartTime;
        EndTime = entity.EndTime;
        PlayTime = entity.PlayTime;
        CourtId = entity.CourtId;
        CourtName = entity.CourtName;
        Location = entity.Location;
        ConsumptionPatterns = entity.ConsumptionPatterns;
        Amount = entity.Amount;
        MinPeople = entity.MinPeople;
        MaxPeople = entity.MaxPeople;
        AlternatePeople = entity.AlternatePeople;
        LevelGroup = entity.LevelGroup;
        OtherInfo = entity.OtherInfo;
        MemberId = entity.MemberId;
        MemberName = memberName;
        IsPrivate = entity.IsPrivate;
        CreateTime = entity.CreateTime;
        UpdateTime = entity.UpdateTime;
        JoinedMembers = entity.Members.Select(m => new GroupMember
        {
            MemberId = m.MemberId,
            JoinTime = m.JoinTime,
            LineUserId = m.Member?.LineUserId ?? string.Empty,
            DisplayName = m.Member?.MemberName ?? string.Empty,
            PictureUrl = m.Member?.PictureUrl ?? string.Empty
        }).ToList();
    }
}

