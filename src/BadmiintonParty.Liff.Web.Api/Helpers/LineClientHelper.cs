namespace BadmiintonParty.Liff.Web.Api.Helpers;

using BadmiintonParty.Liff.Web.Api.Models;
using System.Text.Json;

public class LineClientHelper
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string ClientName = "LineHttpClient";
    private const string VerifyTokenUrlTemplate = "/oauth2/v2.1/verify?access_token={0}";

    public LineClientHelper(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<LineVerifyResponse?> VerifyTokenAsync(string token)
    {
        using var client = _httpClientFactory.CreateClient(ClientName);
        var response = await client.GetAsync(string.Format(VerifyTokenUrlTemplate, token));

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LineVerifyResponse>(jsonResponse);
        }

        return null;
    }
}
