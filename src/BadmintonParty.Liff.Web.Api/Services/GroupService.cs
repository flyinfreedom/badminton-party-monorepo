namespace BadmintonParty.Liff.Web.Api.Services;

using BadmintonParty.Liff.Web.Api.Contexts;
using BadmintonParty.Liff.Web.Api.Entities;
using BadmintonParty.Liff.Web.Api.Enums;
using BadmintonParty.Liff.Web.Api.Exceptions;
using BadmintonParty.Liff.Web.Api.Extensions;
using BadmintonParty.Liff.Web.Api.Models;
using BadmintonParty.Liff.Web.Api.Repositories;
using System.Collections.Concurrent;

public class GroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IMemberGroupRepository _memberGroupRepository;
    private readonly GroupMembersDistribution _groupMembersDistribution;
    private readonly ICourtRepository _courtRepository;

    public GroupService(
        IGroupRepository groupRepository,
        IMemberGroupRepository memberGroupRepository,
        GroupMembersDistribution groupMembersDistribution,
        ICourtRepository courtRepository)
    {
        _groupRepository = groupRepository;
        _memberGroupRepository = memberGroupRepository;
        _groupMembersDistribution = groupMembersDistribution;
        _courtRepository = courtRepository;
    }

    public Task<GroupEntity> GetGroupById(string groupId) => _groupRepository.GetGroupById(groupId);

    public async Task<MyCurrentGroups> GetMyCurrentGroup(string memberId)
    {
        var now = DateTime.UtcNow;
        var memberGroups = await _memberGroupRepository.GetCurrentMemberGroupAsync(memberId, DateTime.UtcNow.ToTaipeiTime().ToYearMonthInteger());
        
        var createdGroupIds = memberGroups.SelectMany(mg => mg.CreatedGroups)
            .Where(g => g.EndTime.ToUniversalTime() > now && g.GroupStatus == GroupStatus.Opened)
            .Select(g => g.GroupId)
            .ToHashSet();

        var joinedGroupIds = memberGroups.SelectMany(mg => mg.JoinedGroups)
            .Where(g => g.EndTime.ToUniversalTime() > now && g.GroupStatus == GroupStatus.Opened)
            .Select(g => g.GroupId)
            .ToHashSet();

        return new MyCurrentGroups
        {
            CreatedGroups = _groupRepository.GetGroupByBatchGetItem(createdGroupIds).Select(g => new GroupResponse(g)).ToList(),
            JoinedGroups = _groupRepository.GetGroupByBatchGetItem(joinedGroupIds).Select(g => new GroupResponse(g)).ToList()
        };
    }

    public async Task<MyCurrentGroups> GetMyHistoryGroup(string memberId, int startTimeYearMonth)
    {
        var now = DateTime.UtcNow;
        var memberGroups = await _memberGroupRepository.GetMemberGroupAsync(memberId, startTimeYearMonth);

        if (memberGroups is null) return new MyCurrentGroups();

        var createdGroupIds = memberGroups.CreatedGroups
            .Where(g => g.EndTime.ToUniversalTime() < now || g.GroupStatus == GroupStatus.Closed)
            .Select(g => g.GroupId)
            .ToHashSet();

        var joinedGroupIds = memberGroups.JoinedGroups
            .Where(g => g.EndTime.ToUniversalTime() < now || g.GroupStatus == GroupStatus.Closed)
            .Select(g => g.GroupId)
            .ToHashSet();

        return new MyCurrentGroups
        {
            CreatedGroups = _groupRepository.GetGroupByBatchGetItem(createdGroupIds).Select(g => new GroupResponse(g)).ToList(),
            JoinedGroups = _groupRepository.GetGroupByBatchGetItem(joinedGroupIds).Select(g => new GroupResponse(g)).ToList()
        };
    }

    public Task<List<GroupMember>> JoinGroup(string groupId, MemberProfile memberProfile)
    {
        var domain = _groupMembersDistribution.GetGroupMembers(groupId);
        return Task.FromResult(domain.HandleGroupMembers(OperationType.Increase, new GroupMember
        {
            DisplayName = memberProfile.DisplayName,
            PictureUrl = memberProfile.PictureUrl,
            LineUserId = memberProfile.LineUserId,
            MemberId = memberProfile.MemberId,
            JoinTime = DateTime.UtcNow
        }));
    }

    public Task<List<GroupMember>> MinusOneGroupMember(string groupId, string memberId)
    {
        var domain = _groupMembersDistribution.GetGroupMembers(groupId);
        return Task.FromResult(domain.HandleGroupMembers(OperationType.Decrease, new GroupMember { MemberId = memberId }));
    }

    public Task<List<GroupMember>> LeaveGroup(string groupId, string memberId)
    {
        var domain = _groupMembersDistribution.GetGroupMembers(groupId);
        return Task.FromResult(domain.HandleGroupMembers(OperationType.Clear, new GroupMember { MemberId = memberId }));
    }

    public async Task<GroupEntity> CreateGroup(GroupFormRequest request, IUserContext userContext)
    {
        var now = DateTime.UtcNow;
        var courtId = string.IsNullOrWhiteSpace(request.CourtId) ? null : request.CourtId;
        if (courtId != null)
        {
            var court = await _courtRepository.GetCourtById(courtId);
            if (court == null)
            {
                courtId = null;
            }
        }

        var entity = new GroupEntity
        {
            GroupId = Guid.NewGuid().ToString(),
            GroupName = request.GroupName,
            GroupStatus = GroupStatus.Opened,
            Avatar = userContext.PictureUrl,
            StartTime = request.StartTime.ToUniversalTime(),
            EndTime = request.StartTime.AddHours(request.PlayTime).ToUniversalTime(),
            PlayTime = request.PlayTime,
            CourtId = courtId,
            CourtName = request.CourtName,
            Location = request.Location,
            ConsumptionPatterns = request.ConsumptionPatterns,
            Amount = request.Amount,
            MinPeople = request.MinPeople,
            MaxPeople = request.MaxPeople,
            AlternatePeople = request.AlternatePeople,
            LevelGroup = request.LevelGroup,
            IsPrivate = request.IsPrivate,
            OtherInfo = request.OtherInfo,
            MemberId = userContext.MemberId,
            CreateTime = now,
            UpdateTime = now,
            Members = []
        };

        await _groupRepository.CreateGroup(entity);
        await _memberGroupRepository.CreateGroupAsync(entity.MemberId, entity.StartTime.ToYearMonthInteger(), entity);
        return entity;
    }

    public async Task<GroupEntity> UpdateGroup(string groupId, string memberId, GroupFormRequest request)
    {
        var group = await _groupRepository.GetGroupById(groupId);
        if (group.MemberId != memberId) throw new CustomException("您沒有更新這個羽球團的權限");

        if (group.Members.Count > request.MaxPeople + request.AlternatePeople)
            throw new CustomException("上限人數加候補人數不可小於當前報名人數");

        var hasDateChange = group.StartTime != request.StartTime.ToUniversalTime() || group.PlayTime != request.PlayTime;
        var originalStartYearMonth = group.StartTime.ToYearMonthInteger();

        var courtId = string.IsNullOrWhiteSpace(request.CourtId) ? null : request.CourtId;
        if (courtId != null)
        {
            var court = await _courtRepository.GetCourtById(courtId);
            if (court == null)
            {
                courtId = null;
            }
        }

        group.GroupName = request.GroupName;
        group.StartTime = request.StartTime.ToUniversalTime();
        group.EndTime = request.StartTime.AddHours(request.PlayTime).ToUniversalTime();
        group.PlayTime = request.PlayTime;
        group.CourtId = courtId;
        group.CourtName = request.CourtName;
        group.Location = request.Location;
        group.ConsumptionPatterns = request.ConsumptionPatterns;
        group.Amount = request.Amount;
        group.LevelGroup = request.LevelGroup;
        group.MinPeople = request.MinPeople;
        group.MaxPeople = request.MaxPeople;
        group.AlternatePeople = request.AlternatePeople;
        group.OtherInfo = request.OtherInfo;
        group.IsPrivate = request.IsPrivate;
        group.UpdateTime = DateTime.UtcNow;

        if (!await _groupRepository.UpdateGroup(group)) throw new CustomException("更新失敗");

        if (hasDateChange)
        {
            await _memberGroupRepository.LeaveGroupAsync(memberId, originalStartYearMonth, group.GroupId);
            await _memberGroupRepository.CreateGroupAsync(memberId, group.StartTime.ToYearMonthInteger(), group);

            await _memberGroupRepository.UpdateAllJoinedGroup(
                group.Members.Where(m => m.MemberId != memberId).Select(m => m.MemberId).ToHashSet(),
                originalStartYearMonth,
                group.StartTime.ToYearMonthInteger(),
                group);
        }

        return group;
    }

    public async Task<bool> CloseGroup(string groupId, string memberId)
    {
        var group = await _groupRepository.GetGroupById(groupId);
        if (group.MemberId != memberId) throw new CustomException("您沒有更新這個羽球團的權限");

        await _memberGroupRepository.CloseCreatedGroup(memberId, group.StartTime.ToYearMonthInteger(), groupId);
        await _memberGroupRepository.CloseAllJoinedGroup(
            group.Members.Select(m => m.MemberId).ToHashSet(),
            group.StartTime.ToYearMonthInteger(),
            groupId);

        return await _groupRepository.CloseGroup(groupId);
    }
}

public class GroupMembersDistribution
{
    private readonly ConcurrentDictionary<string, GroupMembersDomain> _groupMembersDictionary = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly Mutex _mutex = new(false, nameof(GroupMembersDistribution));

    public GroupMembersDistribution(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public GroupMembersDomain GetGroupMembers(string groupId)
    {
        _mutex.WaitOne();
        try
        {
            return _groupMembersDictionary.GetOrAdd(groupId, key =>
            {
                using var scope = _serviceProvider.CreateScope();
                var groupRepository = scope.ServiceProvider.GetRequiredService<IGroupRepository>();
                var group = groupRepository.GetGroupById(key).GetAwaiter().GetResult();
                var joinedMembers = group.Members.Select(m => new GroupMember
                {
                    MemberId = m.MemberId,
                    JoinTime = m.JoinTime,
                    LineUserId = m.Member?.LineUserId ?? string.Empty,
                    DisplayName = m.Member?.MemberName ?? string.Empty,
                    PictureUrl = m.Member?.PictureUrl ?? string.Empty
                }).ToList();
                return new GroupMembersDomain(key, joinedMembers, group.MaxPeople, group.AlternatePeople, _serviceProvider, this);
            });
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    public void ReleaseMemberPoint(string groupId)
    {
        _mutex.WaitOne();
        try
        {
            _groupMembersDictionary.TryRemove(groupId, out _);
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }
}

public class GroupMembersDomain : IDisposable
{
    private readonly Timer _timer;
    private readonly List<GroupMember> _groupMembers;
    private readonly GroupMembersDistribution _groupMembersDistribution;
    private readonly string _groupId;
    private readonly int _maxPeople;
    private readonly int _alternatePeople;
    private readonly Mutex _mutex;
    private readonly IServiceProvider _serviceProvider;
    private bool _isDisposed;

    public GroupMembersDomain(
        string groupId,
        List<GroupMember> groupMembers,
        int maxPeople,
        int alternatePeople,
        IServiceProvider serviceProvider,
        GroupMembersDistribution groupMembersDistribution)
    {
        _groupId = groupId;
        _groupMembers = groupMembers;
        _mutex = new Mutex(false, groupId);
        _groupMembersDistribution = groupMembersDistribution;
        _maxPeople = maxPeople;
        _alternatePeople = alternatePeople;
        _timer = new Timer(_ => Dispose(), null, 10000, Timeout.Infinite);
        _serviceProvider = serviceProvider;
    }

    public List<GroupMember> HandleGroupMembers(OperationType type, GroupMember groupMember)
    {
        if (type == OperationType.Increase && _groupMembers.Count >= _maxPeople + _alternatePeople)
            throw new CustomException("已額滿");

        if (type == OperationType.Decrease && _groupMembers.All(m => m.MemberId != groupMember.MemberId))
            throw new CustomException("成員不存在");

        _mutex.WaitOne();
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var groupRepository = scope.ServiceProvider.GetRequiredService<IGroupRepository>();
            var result = type switch
            {
                OperationType.Increase => groupRepository.JoinGroup(_groupId, groupMember).GetAwaiter().GetResult(),
                OperationType.Decrease => groupRepository.MinusOne(_groupId, groupMember.MemberId).GetAwaiter().GetResult(),
                OperationType.Clear => groupRepository.LeaveGroup(_groupId, groupMember.MemberId).GetAwaiter().GetResult(),
                _ => false
            };

            if (!result) throw new CustomException("更新失敗");

            switch (type)
            {
                case OperationType.Increase: _groupMembers.Add(groupMember); break;
                case OperationType.Decrease: _groupMembers.RemoveAt(_groupMembers.FindLastIndex(m => m.MemberId == groupMember.MemberId)); break;
                case OperationType.Clear: _groupMembers.RemoveAll(m => m.MemberId == groupMember.MemberId); break;
            }
        }
        finally
        {
            _mutex.ReleaseMutex();
        }

        return _groupMembers.OrderBy(m => m.JoinTime).ToList();
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            _timer.Dispose();
            _groupMembersDistribution.ReleaseMemberPoint(_groupId);
            GC.SuppressFinalize(this);
        }
    }

    ~GroupMembersDomain() => Dispose();
}
