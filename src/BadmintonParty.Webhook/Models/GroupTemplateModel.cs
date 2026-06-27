using BadmintonParty.Infrastructure.Entities;
using BadmintonParty.Infrastructure.Enums;
using BadmintonParty.Infrastructure.Extensions;

namespace BadmintonParty.Webhook.Models
{
    public class GroupTemplateModel
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string Avatar { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int PlayTime { get; set; }
        public string CourtName { get; set; }
        public ConsumptionPatterns ConsumptionPatterns { get; set; }
        public int Amount { get; set; }
        public int MaxPeople { get; set; }
        public LevelGroup LevelGroup { get; set; }
        public int JoinedMembersCount { get; set; }

        public GroupTemplateModel(GroupEntity entity)
        {
            GroupId = entity.GroupId;
            GroupName = entity.GroupName;
            Avatar = entity.Avatar;
            StartTime = ConvertToTaipieTimeZone(entity.StartTime);
            EndTime = ConvertToTaipieTimeZone(entity.EndTime);
            PlayTime = entity.PlayTime;
            CourtName = entity.CourtName;
            ConsumptionPatterns = entity.ConsumptionPatterns;
            Amount = entity.Amount;
            MaxPeople = entity.MaxPeople;
            LevelGroup = entity.LevelGroup;
            JoinedMembersCount = entity.Members.Count;
        }

        private DateTime ConvertToTaipieTimeZone(DateTime parsedDateTime)
            => DateTime.SpecifyKind(parsedDateTime, DateTimeKind.Utc).ToTaipeiTime();
    }
}
