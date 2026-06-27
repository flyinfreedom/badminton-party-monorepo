using BadmintonParty.Infrastructure.Enums;
using Line.Messaging;
using Line.Messaging.Webhooks;
using BadmintonParty.Webhook.Services;

namespace BadmintonParty.Webhook.Applications
{
    public class TextEventHandler
    {
        private readonly GroupService _groupService;
        private readonly ILogger<TextEventHandler> _logger;

        public TextEventHandler(GroupService groupService, ILogger<TextEventHandler> logger)
        {
            _groupService = groupService;
            _logger = logger;
        }

        public async Task SetReply(string lineUserId, string text, List<ISendMessage> messages)
        {
            _logger.LogWarning($"{lineUserId}:{text}");
            switch (text)
            {
                case "我的羽球團":
                    messages.Add(await HandleMyBadmintonGroup(lineUserId));
                    break;
                case "使用說明":
                    messages.Add(HandleUsersGuide());
                    break;
            }
        }

        private async Task<ISendMessage> HandleMyBadmintonGroup(string lineUserId)
        {
            var userGroups = await _groupService.GetUserCurrnetGroup(lineUserId);
            
            _logger.LogWarning($"{userGroups.Count}");

            if (userGroups is null || !userGroups.Any()) 
            {
                return new TextMessage("您目前沒有加入或創立任何的羽球團。");
            }

            var columns = new List<CarouselColumn>();

            foreach (var group in userGroups)
            {
                var timeText = $"時間：{group.StartTime.ToString("yyyy/MM/dd")} {group.StartTime.ToString("HH")}:00~{group.EndTime.ToString("HH")}:00";
                var courtText = $"球館：{group.CourtName}";
                var consumptionPatternsText = "費用：";
                consumptionPatternsText += group.ConsumptionPatterns == ConsumptionPatterns.Fixed
                    ? string.Format("{0:N0}", group.Amount)
                    : group.ConsumptionPatterns == ConsumptionPatterns.Free ? "免費" : "平分";

                var description = $"{timeText}\n{courtText}\n{consumptionPatternsText}  [報名人數：{group.JoinedMembersCount} / {group.MaxPeople}]";

                columns.Add(new CarouselColumn(description, group.Avatar, group.GroupName, new List<ITemplateAction>()
                {
                    new UriTemplateAction("詳細內容", $"https://liff.line.me/2003919402-XGBjznML/group/{group.GroupId}/view")
                }));
            }

            return new TemplateMessage($"您目前有 {userGroups.Count} 場羽球團", new CarouselTemplate(columns));
        }

        private ISendMessage HandleUsersGuide()
        {
            var imageUrl = "https://badminton-party.org/api/member/avatar/UsersGuide.jpg";
            return new ImageMessage(imageUrl, imageUrl);
        }
    }
}
