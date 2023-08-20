using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
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

[DurableTask(nameof(DownloadOrchestrator))]
public class DownloadOrchestrator : TaskOrchestrator<string, string>
{
    public override async Task<string> RunAsync(TaskOrchestrationContext context, string input)
    {
        ILogger logger = context.CreateReplaySafeLogger<DownloadOrchestrator>();
        return await context.CallDownloadDataAsync(input);
    }
}

[DurableTask(nameof(DownloadData))]
public class DownloadData(ILogger<DownloadDaily> logger) : TaskActivity<string, string>
{
    private readonly ILogger<DownloadDaily> logger = logger;

    public override Task<string> RunAsync(TaskActivityContext context, string input)
    {
        return Task.FromResult("foo");
    }
}

internal record IngestionRecord();
