using BadmintonParty.Infrastructure.Entities;
using BadmintonParty.Infrastructure.Repositories;
using BadmintonParty.Infrastructure.Enums;
using BadmintonParty.Infrastructure.Contexts;
using BadmintonParty.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using BadmintonParty.Liff.Web.Api.Models;
using BadmintonParty.Infrastructure.Models;
using System.Threading.Tasks;

namespace BadmintonParty.Liff.Web.Api.Filters;

public class ApiResponseFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);

        if (result is IResult iResult)
        {
            // 排除檔案傳輸或串流 (例如大頭貼下載)，不作包裝
            var typeName = iResult.GetType().Name;
            if (typeName.Contains("FileStreamHttpResult") ||
                typeName.Contains("FileContentHttpResult") ||
                typeName.Contains("PhysicalFileHttpResult") ||
                typeName.Contains("VirtualFileHttpResult") ||
                typeName.Contains("PushStreamHttpResult"))
            {
                return result;
            }

            // 針對 JSON 類型的 IResult (例如 Results.Ok(data))
            if (iResult is IValueHttpResult valueResult)
            {
                var value = valueResult.Value;

                if (value != null && IsApiResponse(value))
                {
                    return result;
                }

                if (iResult is IStatusCodeHttpResult statusCodeResult && statusCodeResult.StatusCode >= 400)
                {
                    var apiResponse = ApiResponse.Fail("BAD_REQUEST", value?.ToString() ?? "請求錯誤");
                    return Results.Json(apiResponse, statusCode: statusCodeResult.StatusCode);
                }

                return Results.Ok(ApiResponse<object>.Ok(value!));
            }

            // 單純的 StatusCode 類型的 IResult (例如 Results.NoContent())
            if (iResult is IStatusCodeHttpResult statusResult)
            {
                if (statusResult.StatusCode >= 400)
                {
                    var apiResponse = ApiResponse.Fail("ERROR", "操作失敗");
                    return Results.Json(apiResponse, statusCode: statusResult.StatusCode);
                }
                return Results.Ok(ApiResponse<object>.Ok(null));
            }

            return result;
        }

        // 對於直接回傳 DTO、匿名物件、string 的 Endpoint 進行統一包裝
        if (result != null)
        {
            if (IsApiResponse(result))
            {
                return result;
            }
            return ApiResponse<object>.Ok(result);
        }

        return ApiResponse<object>.Ok(null);
    }

    private static bool IsApiResponse(object obj)
    {
        var type = obj.GetType();
        if (type == typeof(ApiResponse)) return true;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResponse<>)) return true;
        return false;
    }
}
