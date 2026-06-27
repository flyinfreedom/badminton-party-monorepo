using BadmintonParty.Infrastructure.Enums;
using BadmintonParty.Infrastructure.Extensions;
using BadmintonParty.Infrastructure.Repositories;
using BadmintonParty.Webhook.Models;

namespace BadmintonParty.Webhook.Services
{
    public class GroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IMemberGroupRepository _memberGroupRepository;
        private readonly IMemberRepository _memberRepository;

        public GroupService(
            IGroupRepository groupRepository, 
            IMemberGroupRepository memberGroupRepository,
            IMemberRepository memberRepository)
        { 
            _groupRepository = groupRepository;
            _memberGroupRepository = memberGroupRepository;
            _memberRepository = memberRepository;
        }

        public async Task<List<GroupTemplateModel>> GetUserCurrnetGroup(string lineUserId)
        {
            var memberId = await _memberRepository.GetMemberIdByLineId(lineUserId);
            if (string.IsNullOrEmpty(memberId))
            {
                return [];
            }
            
            var now = DateTime.UtcNow;
            var memberGroups = await _memberGroupRepository.GetCurrentMemberGroupAsync(memberId, DateTime.UtcNow.ToTaipeiTime().ToYearMonthInteger());
            var createdGroupIds = memberGroups.SelectMany(mg => mg.CreatedGroups)
                .Where(g => g.EndTime.ToUniversalTime() > now && g.GroupStatus == GroupStatus.Opened)
                .Select(g => g.GroupId)
                .ToList();

            var joinedGroupIds = memberGroups.SelectMany(mg => mg.JoinedGroups)
                .Where(g => g.EndTime.ToUniversalTime() > now && g.GroupStatus == GroupStatus.Opened)
                .Select(g => g.GroupId)
                .ToList();

            var groupIds = createdGroupIds.Concat(joinedGroupIds).ToHashSet();
            if (!groupIds.Any())
            {
                return [];
            }

            var groups = await _groupRepository.GetGroupByBatchGetItemAsync(groupIds);
            var result = groups.Select(g => new GroupTemplateModel(g)).ToList();

            return result;
        }
    }
}
