namespace BadmiintonParty.Liff.Web.Api.Endpoints;

using BadmiintonParty.Liff.Web.Api.Contexts;
using BadmiintonParty.Liff.Web.Api.Entities;
using BadmiintonParty.Liff.Web.Api.Hubs;
using BadmiintonParty.Liff.Web.Api.Models;
using BadmiintonParty.Liff.Web.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;

public static class GroupEndpoints
{
    public static void MapGroupEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/group");

        group.MapGet("/", async (GroupService service, IUserContext userContext) => await service.GetMyCurrentGroup(userContext.MemberId));
        
        group.MapGet("/history/{startTimeYearMonth:int}", async (int startTimeYearMonth, GroupService service, IUserContext userContext) => await service.GetMyHistoryGroup(userContext.MemberId, startTimeYearMonth));
        
        group.MapGet("/{groupId}", async (string groupId, GroupService groupService, MemberService memberService) =>
        {
            var entity = await groupService.GetGroupById(groupId);
            var memberIds = entity.JoinedMembers.Select(x => x.MemberId).ToList();
            memberIds.Add(entity.MemberId);
            var members = memberService.GetMembers(memberIds.ToHashSet()).ToList();
            
            foreach (var joinedMember in entity.JoinedMembers)
            {
                var member = members.FirstOrDefault(m => m.MemberId == joinedMember.MemberId);
                if (member is not null)
                {
                    joinedMember.PictureUrl = member.PictureUrl;
                    joinedMember.DisplayName = member.MemberName;
                }
            }

            var creator = members.FirstOrDefault(m => m.MemberId == entity.MemberId);
            return new GroupResponse(entity, creator?.MemberName ?? "");
        });

        group.MapPost("/{groupId}/join", async (string groupId, GroupService service, IUserContext userContext, IHubContext<GroupHub> hubContext) =>
        {
            var result = await service.JoinGroup(groupId, new MemberProfile
            {
                MemberId = userContext.MemberId,
                DisplayName = userContext.DisplayName,
                LineUserId = userContext.LineUserId,
                PictureUrl = userContext.PictureUrl
            });

            await hubContext.Clients.Group($"home_{groupId}").SendAsync("home_group", new { groupId, count = result.Count });
            await hubContext.Clients.Group(groupId).SendAsync("group", result);
            return result;
        });

        group.MapPost("/{groupId}/minus", async (string groupId, GroupService service, IUserContext userContext, IHubContext<GroupHub> hubContext) =>
        {
            var result = await service.MinusOneGroupMember(groupId, userContext.MemberId);
            await hubContext.Clients.Group($"home_{groupId}").SendAsync("home_group", new { groupId, count = result.Count });
            await hubContext.Clients.Group(groupId).SendAsync("group", result);
            return result;
        });

        group.MapPost("/{groupId}/leave", async (string groupId, GroupService service, IUserContext userContext, IHubContext<GroupHub> hubContext) =>
        {
            var result = await service.LeaveGroup(groupId, userContext.MemberId);
            await hubContext.Clients.Group($"home_{groupId}").SendAsync("home_group", new { groupId, count = result.Count });
            await hubContext.Clients.Group(groupId).SendAsync("group", result);
            return result;
        });

        group.MapPost("/", async (GroupFormRequest request, GroupService groupService, MemberService memberService, IUserContext userContext) =>
        {
            var entity = await groupService.CreateGroup(request, userContext);
            await memberService.HandleRecentOpening(userContext.MemberId, request);
            return entity.GroupId;
        });

        group.MapPut("/{groupId}", async (string groupId, GroupFormRequest request, GroupService groupService, MemberService memberService, IUserContext userContext) =>
        {
            var result = await groupService.UpdateGroup(groupId, userContext.MemberId, request);
            await memberService.HandleRecentOpening(userContext.MemberId, request);
            return result;
        });

        group.MapPost("/{groupId}/close", async (string groupId, GroupService service, IUserContext userContext) => await service.CloseGroup(groupId, userContext.MemberId));

        group.MapPost("/remove_group_member/{groupId}/{memberId}", async (string groupId, string memberId, GroupService groupService, IUserContext userContext, IHubContext<GroupHub> hubContext) =>
        {
            var group = await groupService.GetGroupById(groupId);
            if (group.MemberId != userContext.MemberId) return Results.Forbid();
            
            var result = await groupService.MinusOneGroupMember(groupId, memberId);
            await hubContext.Clients.Group($"home_{groupId}").SendAsync("home_group", new { groupId, count = result.Count });
            await hubContext.Clients.Group(groupId).SendAsync("group", result);
            return Results.Ok(result);
        });
    }
}
