using Azure.Storage.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;



var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((ctx, services) =>
    {
        var cfg = ctx.Configuration;
        services.AddSingleton(_ => new QueueClient(
            cfg["AZURE_STORAGE_CONN"],
            cfg["QUEUE_NAME"],
            new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 }));
    })
    .Build();

host.Run();
