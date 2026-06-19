namespace BadmintonParty.Liff.Web.Api.Endpoints;

using BadmintonParty.Liff.Web.Api.Helpers;
using BadmintonParty.Liff.Web.Api.Models;
using BadmintonParty.Liff.Web.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

public static class PublicEndpoints
{
    public static void MapPublicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/public");

        group.MapPost("/member/init", async (GetMemberProfileRequest request, HttpContext context, LineClientHelper lineClientHelper, IdentityService identityService) =>
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

        group.MapGet("/avatar/{imageKey}", async (string imageKey, GcsHelper gcsHelper) =>
        {
            var stream = await gcsHelper.GetFileAsync(imageKey);
            return stream is null ? Results.NotFound() : Results.File(stream, "image/jpeg");
        });
    }
}

