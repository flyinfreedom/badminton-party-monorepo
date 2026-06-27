using BadmintonParty.Infrastructure.Entities;
using BadmintonParty.Infrastructure.Repositories;
using BadmintonParty.Infrastructure.Enums;
using BadmintonParty.Infrastructure.Extensions;
namespace BadmintonParty.Liff.Web.Api.Middlewares;

using BadmintonParty.Infrastructure.Contexts;
using BadmintonParty.Liff.Web.Api.Contexts;
using BadmintonParty.Liff.Web.Api.Helpers;
using BadmintonParty.Liff.Web.Api.Models;
using BadmintonParty.Infrastructure.Models;
using BadmintonParty.Liff.Web.Api.Services;
using Microsoft.Extensions.Logging;

public class AuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, LineClientHelper lineClientHelper, IdentityService identityService, IUserContext userContext)
    {
        context.Request.Headers.TryGetValue("Authorization", out var token);
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 401;
            return;
        }

        var verifyResult = await lineClientHelper.VerifyTokenAsync(token!);
        if (verifyResult is null)
        {
            context.Response.StatusCode = 401;
            return;
        }

        var profile = identityService.GetMemberProfileFromCache(token!);
        if (profile is null)
        {
            context.Response.StatusCode = 701;
            return;
        }

        userContext.SetUserProfile(profile);
        await next(context);
    }
}



public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exceptions.CustomException ex)
        {
            logger.LogWarning(ex, "Business exception occurred: {Message}", ex.Message);
            context.Response.StatusCode = ex.StatusCode;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail("BUSINESS_ERROR", ex.Message));
        }
        catch (System.Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail("SYSTEM_ERROR", ex.Message));
        }
    }
}

