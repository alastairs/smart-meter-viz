using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((host, function) =>
    {
        function.Services.AddHttpClient();
        function.Services.AddHttpClient("n3rgy", http =>
        {
            http.BaseAddress = new Uri("https://consumer-api.data.n3rgy.com");
            http.DefaultRequestHeaders.Add("Authorization", "");
            http.DefaultRequestHeaders.Add("User-Agent", "uk.yellowhouse.n3rgy.import");
            http.DefaultRequestHeaders.Add("X-Admin-Contact", "");
        });
    })
    .ConfigureServices((host, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights();
        services.AddAzureClients(azure =>
        {
            azure.AddTableServiceClient(
                host.Configuration.GetSection("AzureWebJobsStorage"))
                .WithName("AzureWebJobsStorage");
        });
    })
    .Build();

host.Run();
