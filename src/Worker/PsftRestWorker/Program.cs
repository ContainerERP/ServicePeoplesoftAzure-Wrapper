using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Psft.Shared;
using System;
using System.Net.Http;
using System.Net;

Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, cfg) =>
    {
        // Loads appsettings.json and appsettings.{Environment}.json
        cfg.AddJsonFile("appsettings.json", optional: false)
           .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development"}.json",
                        optional: true)
           .AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        var cfg = ctx.Configuration;

        // ---- Queue client registration (DI will know how to make QueueClient) ----
        services.AddSingleton(sp =>
        {
            // EXACT key name from appsettings.json
            var conn = cfg["AZURE_STORAGE_CONNECTION_STRING"];
            var qName = cfg["QUEUE_NAME"];                  // e.g., "psft-work-items"

            // Fail fast if either is missing
            if (string.IsNullOrWhiteSpace(conn)) throw new InvalidOperationException("AZURE_STORAGE_CONNECTION_STRING not set");
            if (string.IsNullOrWhiteSpace(qName)) throw new InvalidOperationException("QUEUE_NAME not set");

            return new QueueClient(conn, qName, new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
        });

        // ServiceNow status updater used inside the worker
        services.AddHttpClient();
        services.AddSingleton<ITicketSink, ServiceNowTicketUpdater>();

        // PsftRestWrapper client (to call your /api/migrate/run-ticket/{sys_id})
      /*  services.AddHttpClient<PsftWrapperClient>(c =>
        {
            c.BaseAddress = new Uri(cfg["PsftWrapper:BaseUrl"]); // http://localhost:60073
            c.Timeout = TimeSpan.FromMilliseconds(int.TryParse(cfg["PsftWrapper:TimeoutMs"], out var ms) && ms > 0 ? ms : 480000);
        });
      */

        services.AddHttpClient<PsftWrapperClient>(c =>
        {
            c.BaseAddress = new Uri(cfg["PsftWrapper:BaseUrl"]);   // http://localhost:60073
            c.Timeout = TimeSpan.FromSeconds(60);                  // Reasonable upper bound per run
        })
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(4),    // Prevent stale DNS/TCP
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2), // Close unused sockets sooner
    MaxConnectionsPerServer = 20,                          // Safe concurrency
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
});

        // Background loop
        services.AddHostedService<QueueWorker>();
    })
    .Build()
    .Run();
