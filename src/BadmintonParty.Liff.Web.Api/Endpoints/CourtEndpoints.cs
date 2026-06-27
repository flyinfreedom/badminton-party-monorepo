using BadmintonParty.Infrastructure.Entities;
using BadmintonParty.Infrastructure.Repositories;
using BadmintonParty.Infrastructure.Enums;
using BadmintonParty.Infrastructure.Contexts;
using BadmintonParty.Infrastructure.Extensions;
using BadmintonParty.Infrastructure.Models;
namespace BadmintonParty.Liff.Web.Api.Endpoints;

using BadmintonParty.Liff.Web.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

public static class CourtEndpoints
{
    public static void MapCourtEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/court");
        
        group.MapGet("/", async (CourtService service) => await service.GetCourtsAsync());
        group.MapGet("/{courtId}", async (string courtId, CourtService service) => await service.GetCourtById(courtId));
        group.MapGet("/groups/{courtId}", async (string courtId, CourtService service) => await service.GetGroupsByCourtId(courtId));
    }
}

