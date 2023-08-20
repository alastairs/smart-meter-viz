using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;

namespace YellowHouse.N3rgy;
public class DownloadDaily(IHttpClientFactory httpClientFactory, IAzureClientFactory<TableClient> tableClient)
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IAzureClientFactory<TableClient> _tableClient = tableClient;

    [Function(nameof(DownloadDaily)), FixedDelayRetry(maxRetryCount: 5, delayInterval: "00:00:10")]
    [TableOutput("ingestion-record", Connection = "AzureWebJobsStorage")]
    public TableEntity Run(
        [TimerTrigger("0 0 1 * * *")] TimerInfo timerInfo,
        FunctionContext context,
        CancellationToken cancellation)
    {
        var logger = context.GetLogger<DownloadDaily>();

        logger.LogInformation("C# Timer trigger function {FunctionName} executed at: {ExecutionStartedUtc}", nameof(DownloadDaily), DateTime.UtcNow);
        logger.LogInformation("Next timer schedule at: {NextRunScheduleUtc}", timerInfo.ScheduleStatus.Next);

        var client = _httpClientFactory.CreateClient("n3rgy");
        client.GetStreamAsync("");

        return new TableEntity();
    }
}

internal record IngestionRecord(

);
