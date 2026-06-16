namespace BadmiintonParty.Liff.Web.Api.Hubs;

using Microsoft.AspNetCore.SignalR;

public class GroupHub : Hub
{
    public Task JoinHomeGroup(List<string> groupIds)
    {
        var tasks = groupIds.Select(groupId => Groups.AddToGroupAsync(Context.ConnectionId, $"home_{groupId}"));
        return Task.WhenAll(tasks);
    }

    public Task LeaveHomeGroup(List<string> groupIds)
    {
        var tasks = groupIds.Select(groupId => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"home_{groupId}"));
        return Task.WhenAll(tasks);
    }

    public Task JoinGroup(string groupId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, groupId);
    }

    public Task LeaveGroup(string groupId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
    }
}
