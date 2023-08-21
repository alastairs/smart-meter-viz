using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((host, services) =>
    {
        services.AddHttpClient();
        services.AddHttpClient("n3rgy", http =>
        {
            http.BaseAddress = new Uri("https://consumer-api.data.n3rgy.com");
            http.DefaultRequestHeaders.Add("Authorization", "0CA2F4000056E083".ToUpperInvariant());
            http.DefaultRequestHeaders.Add("User-Agent", "uk.yellowhouse.n3rgy.import");
            http.DefaultRequestHeaders.Add("X-Admin-Contact", "alastair@alastairsmith.me.uk");
        });

        services.AddAzureClients(clients =>
        {
            clients.UseCredential(new DefaultAzureCredential());
            clients.AddTableServiceClient(
                host.Configuration.GetSection("Storage"))
                .WithName("AzureWebJobsStorage");
            clients.AddBlobServiceClient(
                host.Configuration.GetSection("Storage"))
                .WithName("AzureWebJobsStorage");
        });
        services.AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights();
    })
    .Build();

await host.RunAsync();
