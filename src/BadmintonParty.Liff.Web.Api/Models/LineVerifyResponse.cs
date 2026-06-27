using BadmintonParty.Infrastructure.Entities;
using BadmintonParty.Infrastructure.Repositories;
using BadmintonParty.Infrastructure.Enums;
using BadmintonParty.Infrastructure.Contexts;
using BadmintonParty.Infrastructure.Extensions;
using BadmintonParty.Infrastructure.Models;
namespace BadmintonParty.Liff.Web.Api.Models;

using System.Text.Json.Serialization;

public class LineVerifyResponse
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

