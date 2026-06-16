namespace BadmiintonParty.Liff.Web.Api.Converters;

using BadmiintonParty.Liff.Web.Api.Consts;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class LocalTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (DateTime.TryParseExact(s, "yyyy-MM-ddTHH:mm:ss", null, DateTimeStyles.AssumeLocal, out var dt))
            {
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
                return TimeZoneInfo.ConvertTimeToUtc(dt, MyTimeZone.TaipeiTimeZone);
            }
            return dt.ToUniversalTime();
        }
        return reader.GetDateTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(value.ToUniversalTime(), MyTimeZone.TaipeiTimeZone);
        writer.WriteStringValue(local.ToString("yyyy-MM-ddTHH:mm:ss"));
    }
}
