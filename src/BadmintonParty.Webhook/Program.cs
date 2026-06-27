using BadmintonParty.Infrastructure.Contexts;
using BadmintonParty.Infrastructure.Repositories;
using BadmintonParty.Webhook.Applications;
using BadmintonParty.Webhook.Services;
using Line.Messaging;
using Line.Messaging.Webhooks;
using Microsoft.EntityFrameworkCore;

System.AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddOpenApi();

// DB Context
var connectionString = builder.Configuration.GetConnectionString("badminton-party") ?? string.Empty;
if (connectionString.Contains("localhost") || string.IsNullOrEmpty(connectionString))
{
    var fallback = builder.Configuration.GetConnectionString("badminton_party");
    if (!string.IsNullOrEmpty(fallback))
    {
        connectionString = fallback;
    }
}

builder.Services.AddDbContext<BadmintonContext>(options =>
    options.UseNpgsql(connectionString));

// LINE Bot Configuration
builder.Services.AddSingleton<LineMessagingClient>(provider =>
{
    var accessToken = builder.Configuration.GetValue<string>("LINE:AccessToken") ?? string.Empty;
    return new LineMessagingClient(accessToken);
});

// DI Registrations
builder.Services.AddScoped<ILineBotApplication, LineBotApplication>();
builder.Services.AddScoped<TextEventHandler>();
builder.Services.AddScoped<GroupService>();

builder.Services.AddScoped<ICourtRepository, CourtRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IMemberGroupRepository, MemberGroupRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapDefaultEndpoints();

// Minimal API Webhook Endpoint
app.MapPost("/linebot", async (HttpContext httpContext, ILineBotApplication lineBotApplication, IConfiguration configuration) =>
{
    var channelSecret = configuration.GetValue<string>("LINE:ChannelSecret") ?? string.Empty;
    var events = await httpContext.Request.GetWebhookEventsAsync(channelSecret);
    await lineBotApplication.RunAsync(events);
    return Results.NoContent();
});

app.Run();
