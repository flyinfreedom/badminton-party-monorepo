using BadmiintonParty.Liff.Web.Api.Contexts;
using BadmiintonParty.Liff.Web.Api.Converters;
using BadmiintonParty.Liff.Web.Api.Entities;
using BadmiintonParty.Liff.Web.Api.Helpers;
using BadmiintonParty.Liff.Web.Api.Hubs;
using BadmiintonParty.Liff.Web.Api.Middlewares;
using BadmiintonParty.Liff.Web.Api.Models;
using BadmiintonParty.Liff.Web.Api.Repositories;
using BadmiintonParty.Liff.Web.Api.Services;
using ImageMagick;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("LineHttpClient", client =>
{
    client.BaseAddress = new Uri("https://api.line.me");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new LocalTimeConverter());
});

// DI Registrations
builder.Services.AddScoped<IDynamoContext, DynamoContext>();
builder.Services.AddScoped<GcsHelper>();
builder.Services.AddScoped<ICourtRepository, CourtRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IMemberGroupRepository, MemberGroupRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<CourtService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<MemberService>();
builder.Services.AddSingleton<LineClientHelper>();
builder.Services.AddSingleton<IdentityService>();
builder.Services.AddSingleton<GroupMembersDistribution>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Public APIs
var publicApi = app.MapGroup("/api/public");

publicApi.MapPost("/member/init", async (GetMemberProfileRequest request, HttpContext context, LineClientHelper lineClientHelper, IdentityService identityService) =>
{
    if (!context.Request.Headers.TryGetValue("Authorization", out var token))
        return Results.BadRequest("don't carry token");

    var verifyResult = await lineClientHelper.VerifyTokenAsync(token!);
    if (verifyResult is null)
        return Results.BadRequest("token verified fail");

    var profile = await identityService.GetMemberProfile(request);
    identityService.SetMemberProfileToCache(token!, profile, TimeSpan.FromSeconds(verifyResult.ExpiresIn));
    return Results.Ok(profile);
});

publicApi.MapGet("/avatar/{imageKey}", async (string imageKey, GcsHelper gcsHelper) =>
{
    var stream = await gcsHelper.GetFileAsync(imageKey);
    return stream is null ? Results.NotFound() : Results.File(stream, "image/jpeg");
});

// Protected APIs
var api = app.MapGroup("/api");
api.AddEndpointFilter<AuthFilter>();

// Court APIs
var courtApi = api.MapGroup("/court");
courtApi.MapGet("/", async (CourtService service) => await service.GetCourtsAsync());
courtApi.MapGet("/{courtId}", async (string courtId, CourtService service) => await service.GetCourtById(courtId));
courtApi.MapGet("/groups/{courtId}", async (string courtId, CourtService service) => await service.GetGroupsByCourtId(courtId));

// Group APIs
var groupApi = api.MapGroup("/group");
groupApi.MapGet("/", async (GroupService service, IUserContext userContext) => await service.GetMyCurrentGroup(userContext.MemberId));
groupApi.MapGet("/history/{startTimeYearMonth:int}", async (int startTimeYearMonth, GroupService service, IUserContext userContext) => await service.GetMyHistoryGroup(userContext.MemberId, startTimeYearMonth));
groupApi.MapGet("/{groupId}", async (string groupId, GroupService groupService, MemberService memberService) =>
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

groupApi.MapPost("/{groupId}/join", async (string groupId, GroupService service, IUserContext userContext, IHubContext<GroupHub> hubContext) =>
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

groupApi.MapPost("/{groupId}/minus", async (string groupId, GroupService service, IUserContext userContext, IHubContext<GroupHub> hubContext) =>
{
    var result = await service.MinusOneGroupMember(groupId, userContext.MemberId);
    await hubContext.Clients.Group($"home_{groupId}").SendAsync("home_group", new { groupId, count = result.Count });
    await hubContext.Clients.Group(groupId).SendAsync("group", result);
    return result;
});

groupApi.MapPost("/{groupId}/leave", async (string groupId, GroupService service, IUserContext userContext, IHubContext<GroupHub> hubContext) =>
{
    var result = await service.LeaveGroup(groupId, userContext.MemberId);
    await hubContext.Clients.Group($"home_{groupId}").SendAsync("home_group", new { groupId, count = result.Count });
    await hubContext.Clients.Group(groupId).SendAsync("group", result);
    return result;
});

groupApi.MapPost("/", async (GroupFormRequest request, GroupService groupService, MemberService memberService, IUserContext userContext) =>
{
    var entity = await groupService.CreateGroup(request, userContext);
    await memberService.HandleRecentOpening(userContext.MemberId, request);
    return entity.GroupId;
});

groupApi.MapPut("/{groupId}", async (string groupId, GroupFormRequest request, GroupService groupService, MemberService memberService, IUserContext userContext) =>
{
    var result = await groupService.UpdateGroup(groupId, userContext.MemberId, request);
    await memberService.HandleRecentOpening(userContext.MemberId, request);
    return result;
});

groupApi.MapPost("/{groupId}/close", async (string groupId, GroupService service, IUserContext userContext) => await service.CloseGroup(groupId, userContext.MemberId));

groupApi.MapPost("/remove_group_member/{groupId}/{memberId}", async (string groupId, string memberId, GroupService groupService, IUserContext userContext, IHubContext<GroupHub> hubContext) =>
{
    var group = await groupService.GetGroupById(groupId);
    if (group.MemberId != userContext.MemberId) return Results.Forbid();
    
    var result = await groupService.MinusOneGroupMember(groupId, memberId);
    await hubContext.Clients.Group($"home_{groupId}").SendAsync("home_group", new { groupId, count = result.Count });
    await hubContext.Clients.Group(groupId).SendAsync("group", result);
    return Results.Ok(result);
});

// Member APIs
var memberApi = api.MapGroup("/member");
memberApi.MapGet("/", async (MemberService service, IUserContext userContext) =>
{
    var member = await service.GetMember(userContext.MemberId);
    return new { member.MemberName, AvatarUrl = member.PictureUrl };
});

memberApi.MapPut("/name", async ([FromBody] UpdateUserNameRequest request, MemberService service, IUserContext userContext) =>
{
    await service.UpdateUserName(userContext.MemberId, request.Name);
    return request.Name;
});

memberApi.MapPost("/avatar", async (IFormFile file, GcsHelper gcsHelper, MemberService memberService, IUserContext userContext, HttpContext httpContext) =>
{
    if (file == null || file.Length == 0) return Results.BadRequest("file is null or empty");

    var imageKey = Guid.NewGuid().ToString();
    using var memoryStream = new MemoryStream();
    
    try
    {
        using var stream = file.OpenReadStream();
        using var image = new MagickImage(stream);
        uint maxSize = 500;
        uint newWidth, newHeight;
        if (image.Width > image.Height)
        {
            newWidth = maxSize;
            newHeight = (uint)((float)image.Height / image.Width * maxSize);
        }
        else
        {
            newWidth = (uint)((float)image.Width / image.Height * maxSize);
            newHeight = maxSize;
        }
        image.Resize(newWidth, newHeight);
        image.Format = MagickFormat.Jpeg;
        image.Write(memoryStream);
        memoryStream.Position = 0;
    }
    catch
    {
        return Results.BadRequest("圖片格式不支援");
    }

    await gcsHelper.UploadFileAsync(memoryStream, imageKey);
    var imageUrl = $"https://{httpContext.Request.Host}/api/public/avatar/{imageKey}";
    userContext.SetUserPictureUrl(imageUrl);
    await memberService.UpdateUserAvatar(userContext.MemberId, imageUrl);
    
    return Results.Ok(new { avatarUrl = imageUrl });
}).DisableAntiforgery();

app.UseMiddleware<ResponseMiddleware>();
app.MapHub<GroupHub>("/groupHub");

app.Run();

public record UpdateUserNameRequest(string Name);

public class AuthFilter(LineClientHelper lineClientHelper, IdentityService identityService, IUserContext userContext) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        httpContext.Request.Headers.TryGetValue("Authorization", out var token);
        if (string.IsNullOrEmpty(token)) return Results.Unauthorized();

        var verifyResult = await lineClientHelper.VerifyTokenAsync(token!);
        if (verifyResult is null) return Results.Unauthorized();

        var profile = identityService.GetMemberProfileFromCache(token!);
        if (profile is null) return Results.StatusCode(701);

        userContext.SetUserProfile(profile);
        return await next(context);
    }
}
