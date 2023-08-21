using Azure.Storage.Blobs;
using Microsoft.DurableTask;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;

namespace YellowHouse.N3rgy;

[DurableTask(nameof(DownloadDataFile))]
public class DownloadDataFile(
    ILogger<DownloadDaily> logger,
    IHttpClientFactory httpClientFactory,
    IAzureClientFactory<BlobServiceClient> blobClientFactory) : TaskActivity<DownloadDataFile.Input, byte[]>
{
    public record Input(
        DateTime Start,
        DateTime End,
        string Location = ""
    );

    private readonly ILogger<DownloadDaily> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly IAzureClientFactory<BlobServiceClient> _blobClientFactory = blobClientFactory ?? throw new ArgumentNullException(nameof(blobClientFactory));

    public override async Task<byte[]> RunAsync(TaskActivityContext context, Input input)
    {
        var http = _httpClientFactory.CreateClient("n3rgy");

        var blob = await ConfigureBlobClient(_blobClientFactory.CreateClient("AzureWebJobsStorage"), input.Location);
        var stream = await http.GetStreamAsync($"{input.Location}/1?start={input.Start:yyyyMMdd}&end={input.End:yyyyMMdd}&output=csv");

        var response = await blob.UploadAsync(stream, overwrite: true);
        return response.Value.ContentHash;
    }

    private async Task<BlobClient> ConfigureBlobClient(BlobServiceClient blobServiceClient, string location)
    {
        var container = blobServiceClient.GetBlobContainerClient("n3rgy-import");
        _ = await container.CreateIfNotExistsAsync();
        var ingestionDate = DateTime.UtcNow;
        return container.GetBlobClient($"{ingestionDate:yyyy/MM/dd}/{location}.csv");
    }
}

