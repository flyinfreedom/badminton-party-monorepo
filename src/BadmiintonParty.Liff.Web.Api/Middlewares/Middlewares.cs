namespace BadmiintonParty.Liff.Web.Api.Middlewares;

using BadmiintonParty.Liff.Web.Api.Contexts;
using BadmiintonParty.Liff.Web.Api.Helpers;
using BadmiintonParty.Liff.Web.Api.Services;

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

public class ResponseMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exceptions.CustomException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            await context.Response.WriteAsJsonAsync(new { ex.Message });
        }
    }
}
