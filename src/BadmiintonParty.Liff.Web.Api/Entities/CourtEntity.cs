namespace BadmiintonParty.Liff.Web.Api.Entities;

using Amazon.DynamoDBv2.DataModel;

[DynamoDBTable("Courts")]
public class CourtEntity
{
    [DynamoDBHashKey]
    public string CourtId { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public string CourtName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
}
