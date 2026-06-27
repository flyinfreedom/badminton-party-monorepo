namespace BadmintonParty.Liff.Web.Api.Endpoints;

using BadmintonParty.Liff.Web.Api.Contexts;
using BadmintonParty.Liff.Web.Api.Helpers;
using BadmintonParty.Liff.Web.Api.Models;
using BadmintonParty.Liff.Web.Api.Services;
using ImageMagick;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using BadmintonParty.Liff.Web.Api.Filters;

public static class MemberEndpoints
{
    public static void MapMemberEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/member").AddEndpointFilter<ApiResponseFilter>();

        group.MapGet("/", async (MemberService service, IUserContext userContext) =>
        {
            var member = await service.GetMember(userContext.MemberId);
            return new { member.MemberName, AvatarUrl = member.PictureUrl };
        });

        group.MapPut("/name", async ([FromBody] UpdateUserNameRequest request, MemberService service, IUserContext userContext) =>
        {
            await service.UpdateUserName(userContext.MemberId, request.Name);
            return request.Name;
        });

        group.MapPost("/avatar", async (IFormFile file, GcsHelper gcsHelper, MemberService memberService, IUserContext userContext, HttpContext httpContext) =>
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
            var imageUrl = $"https://{httpContext.Request.Host}/api/member/avatar/{imageKey}";
            userContext.SetUserPictureUrl(imageUrl);
            await memberService.UpdateUserAvatar(userContext.MemberId, imageUrl);
            
            return Results.Ok(new { avatarUrl = imageUrl });
        }).DisableAntiforgery();

        group.MapPost("/init", async (GetMemberProfileRequest request, HttpContext context, LineClientHelper lineClientHelper, IdentityService identityService, JwtService jwtService, Microsoft.Extensions.Logging.ILogger<Program> logger) =>
        {
            logger.LogInformation("MemberInit: Start authentication process for LineUserId: {LineUserId}.", request.LineUserId);

            if (!context.Request.Headers.TryGetValue("Authorization", out var token))
            {
                logger.LogWarning("MemberInit: Authorization header is missing in Request.");
                return Results.BadRequest("don't carry token");
            }

            var verifyResult = await lineClientHelper.VerifyTokenAsync(token!);
            if (verifyResult is null)
            {
                logger.LogWarning("MemberInit: LINE Token verification failed. Token starting with: {TokenPart}...", token.ToString().Substring(0, Math.Min(token.ToString().Length, 10)));
                return Results.BadRequest("token verified fail");
            }

            logger.LogInformation("MemberInit: LINE Token verified successfully. Expiration: {ExpiresIn} seconds.", verifyResult.ExpiresIn);

            var profile = await identityService.GetMemberProfile(request);
            var sysToken = jwtService.GenerateToken(profile, TimeSpan.FromSeconds(verifyResult.ExpiresIn));

            logger.LogInformation("MemberInit: System token generated successfully for MemberId: {MemberId}.", profile.MemberId);

            return Results.Ok(new MemberInitResponse
            {
                Profile = profile,
                Token = sysToken
            });
        }).AllowAnonymous();

        group.MapGet("/avatar/{imageKey}", async (string imageKey, GcsHelper gcsHelper) =>
        {
            var stream = await gcsHelper.GetFileAsync(imageKey);
            return stream is null ? Results.NotFound() : Results.File(stream, "image/jpeg");
        }).AllowAnonymous();
    }
}
