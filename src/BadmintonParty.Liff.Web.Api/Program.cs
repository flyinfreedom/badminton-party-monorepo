using BadmintonParty.Liff.Web.Api.Contexts;
using BadmintonParty.Liff.Web.Api.Converters;
using BadmintonParty.Liff.Web.Api.Endpoints;
using BadmintonParty.Liff.Web.Api.Helpers;
using BadmintonParty.Liff.Web.Api.Hubs;
using BadmintonParty.Liff.Web.Api.Middlewares;
using BadmintonParty.Liff.Web.Api.Repositories;
using BadmintonParty.Liff.Web.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BadmintonParty.Liff.Web.Api.Models;

System.AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
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
builder.Services.AddSingleton<JwtService>();
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

var protectedGroup = app.MapGroup("/api");
protectedGroup.AddEndpointFilter<AuthFilter>();
protectedGroup.MapCourtEndpoints();
protectedGroup.MapGroupEndpoints();
protectedGroup.MapMemberEndpoints();

app.UseMiddleware<ResponseMiddleware>();
app.MapHub<GroupHub>("/groupHub");

// 自動執行資料庫遷移
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<BadmintonContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();

public class AuthFilter(JwtService jwtService, IUserContext userContext, ILogger<AuthFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var path = httpContext.Request.Path;
        var method = httpContext.Request.Method;

        logger.LogInformation("AuthFilter: Step 1 - Entry. Path: {Method} {Path}", method, path);

        try
        {
            var endpoint = httpContext.GetEndpoint();
            logger.LogInformation("AuthFilter: Step 2 - Got endpoint. IsNull: {IsNull}", endpoint is null);

            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            {
                logger.LogInformation("AuthFilter: Step 3 - AllowAnonymous. Path: {Method} {Path}", method, path);
                return await next(context);
            }

            logger.LogInformation("AuthFilter: Step 4 - Reading Authorization Header.");
            httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader);
            logger.LogInformation("AuthFilter: Step 5 - Authorization Header Value: '{Header}'", authHeader.ToString());

            if (string.IsNullOrEmpty(authHeader))
            {
                logger.LogInformation("AuthFilter: Step 6 - Auth Header is missing. Returning 401.");
                return Results.Unauthorized();
            }

            var token = authHeader.ToString().Replace("Bearer ", "").Trim();
            logger.LogInformation("AuthFilter: Step 7 - Parsed Token: '{Token}'", token);

            if (string.IsNullOrEmpty(token))
            {
                logger.LogInformation("AuthFilter: Step 8 - Token is empty. Returning 401.");
                return Results.Unauthorized();
            }

            logger.LogInformation("AuthFilter: Step 9 - Calling ValidateToken.");
            var principal = jwtService.ValidateToken(token);
            logger.LogInformation("AuthFilter: Step 10 - ValidateToken finished. Principal IsNull: {IsNull}", principal is null);

            if (principal is null)
            {
                logger.LogInformation("AuthFilter: Step 11 - Principal is null. Returning 401.");
                return Results.Unauthorized();
            }

            logger.LogInformation("AuthFilter: Step 12 - Extracting Claims.");
            var memberId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                           ?? principal.FindFirst("sub")?.Value 
                           ?? principal.FindFirst("nameid")?.Value;
            var lineUserId = principal.FindFirst("LineUserId")?.Value;
            logger.LogInformation("AuthFilter: Step 13 - Extracted Claims - MemberId: '{MemberId}', LineUserId: '{LineUserId}'", memberId, lineUserId);

            if (string.IsNullOrEmpty(memberId) || string.IsNullOrEmpty(lineUserId))
            {
                logger.LogInformation("AuthFilter: Step 14 - Missing claims. Returning 401.");
                return Results.Unauthorized();
            }

            logger.LogInformation("AuthFilter: Step 15 - Success. MemberId: {MemberId}", memberId);

            userContext.SetUserProfile(new MemberProfile
            {
                MemberId = memberId,
                LineUserId = lineUserId,
                DisplayName = principal.FindFirst("DisplayName")?.Value ?? string.Empty,
                PictureUrl = principal.FindFirst("PictureUrl")?.Value ?? string.Empty
            });

            return await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AuthFilter: Critical Exception occurred during InvokeAsync.");
            throw;
        }
    }
}


