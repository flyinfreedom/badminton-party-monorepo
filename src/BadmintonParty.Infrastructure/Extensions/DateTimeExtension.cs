using BadmintonParty.Infrastructure.Consts;

namespace BadmintonParty.Infrastructure.Extensions;

public static class DateTimeExtension
{
    public static int ToYearMonthInteger(this DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc)
        {
            value = TimeZoneInfo.ConvertTimeFromUtc(value.ToUniversalTime(), MyTimeZone.TaipeiTimeZone);
        }
        return Convert.ToInt32(value.ToString("yyyyMM"));
    }

    public static DateTime ToTaipeiTime(this DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc)
        {
            value = TimeZoneInfo.ConvertTimeFromUtc(value.ToUniversalTime(), MyTimeZone.TaipeiTimeZone);
        }
        return value;
    }
}
