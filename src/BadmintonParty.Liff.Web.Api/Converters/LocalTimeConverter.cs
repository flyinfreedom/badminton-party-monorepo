namespace BadmintonParty.Liff.Web.Api.Converters;

using BadmintonParty.Liff.Web.Api.Consts;
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
                return DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
            }
            if (DateTime.TryParse(s, out var parsedDt))
            {
                return DateTime.SpecifyKind(parsedDt, DateTimeKind.Unspecified);
            }
        }
        return reader.GetDateTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var local = value.Kind == DateTimeKind.Utc ? TimeZoneInfo.ConvertTimeFromUtc(value.ToUniversalTime(), MyTimeZone.TaipeiTimeZone) : value;
        writer.WriteStringValue(local.ToString("yyyy-MM-ddTHH:mm:ss"));
    }
}

