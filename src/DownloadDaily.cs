using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;

namespace YellowHouse.N3rgy;

public class DownloadDaily(IHttpClientFactory httpClientFactory, IAzureClientFactory<TableServiceClient> clientFactory)
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly TableServiceClient _tableClient = clientFactory.CreateClient("AzureWebJobsStorage");

    [Function(nameof(DownloadDaily))]
    // [FixedDelayRetry(maxRetryCount: 5, delayInterval: "00:00:10")]
    // [TableOutput("IngestionRecord", Connection = "AzureWebJobsStorage")]
    public TableEntity Run(
        [TimerTrigger("0 0 1 * * *")] TimerInfo timerInfo,
        [DurableClient] DurableTaskClient orchestrator,
        FunctionContext context,
        CancellationToken cancellation)
    {
        var logger = context.GetLogger<DownloadDaily>();

        logger.LogInformation("C# Timer trigger function {FunctionName} executed at: {ExecutionStartedUtc}", nameof(DownloadDaily), DateTime.UtcNow);
        logger.LogInformation("Next timer schedule at: {NextRunScheduleUtc}", timerInfo.ScheduleStatus.Next);

        orchestrator.ScheduleNewOrchestrationInstanceAsync(nameof(DownloadOrchestrator), new DownloadOrchestrator.Input(
            timerInfo.ScheduleStatus.Next.AddDays(-1),
            timerInfo.ScheduleStatus.Next,
            [
                "electricity/consumption",
                "electricity/tariff",
                "gas/consumption",
                "gas/tariff",
            ]), cancellation);

        return new TableEntity();
    }
}
