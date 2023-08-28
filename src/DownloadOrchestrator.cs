using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace YellowHouse.N3rgy;

[DurableTask(nameof(DownloadOrchestrator))]
public class DownloadOrchestrator : TaskOrchestrator<DownloadOrchestrator.Input, byte[][]>
{
    public record Input(
        DateTime Start,
        DateTime End,
        string[] Sources);

    public override async Task<byte[][]> RunAsync(TaskOrchestrationContext context, Input input)
    {
        ILogger logger = context.CreateReplaySafeLogger<DownloadOrchestrator>();
        logger.LogInformation("Starting orchestrated download for {ItemsToDownload}", input);

        var download = new DownloadDataFile.Input(input.Start, input.End);
        return await Task.WhenAll(input.Sources.Select(item => context.CallDownloadDataFileAsync(download with { Location = item })));
    }
}
