using BadmintonParty.Liff.Web.Api.Contexts;
using BadmintonParty.Liff.Web.Api.Converters;
using BadmintonParty.Liff.Web.Api.Endpoints;
using BadmintonParty.Liff.Web.Api.Helpers;
using BadmintonParty.Liff.Web.Api.Hubs;
using BadmintonParty.Liff.Web.Api.Middlewares;
using BadmintonParty.Liff.Web.Api.Repositories;
using BadmintonParty.Liff.Web.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Aspire Service Defaults
builder.AddServiceDefaults();

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

// DB Context
builder.Services.AddDbContext<BadmintonContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("badminton-party")));

// DI Registrations
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

app.MapDefaultEndpoints();

// Map Endpoints
app.MapPublicEndpoints();

var protectedGroup = app.MapGroup("/api");
protectedGroup.AddEndpointFilter<AuthFilter>();
protectedGroup.MapCourtEndpoints();
protectedGroup.MapGroupEndpoints();
protectedGroup.MapMemberEndpoints();

app.UseMiddleware<ResponseMiddleware>();
app.MapHub<GroupHub>("/groupHub");

app.Run();

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

