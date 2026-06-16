namespace BadmiintonParty.Liff.Web.Api.Helpers;

using Google.Cloud.Storage.V1;
using System.IO;

public class GcsHelper
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;

    public GcsHelper(IConfiguration configuration)
    {
        _storageClient = StorageClient.Create();
        _bucketName = configuration["GCS:BucketName"] ?? "badminton-party-image";
    }

    public async Task UploadFileAsync(Stream fileStream, string objectName)
    {
        await _storageClient.UploadObjectAsync(_bucketName, objectName, null, fileStream);
    }

    public async Task<Stream?> GetFileAsync(string objectName)
    {
        try
        {
            var stream = new MemoryStream();
            await _storageClient.DownloadObjectAsync(_bucketName, objectName, stream);
            stream.Position = 0;
            return stream;
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
