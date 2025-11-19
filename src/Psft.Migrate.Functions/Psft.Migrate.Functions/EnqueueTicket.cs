using System.Net;
using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Psft.Shared; // <-- make sure this project references Psft.Shared

namespace Psft.Migrate.Functions;

public sealed class EnqueueTicket
{
    private readonly QueueClient _queue;
    private readonly ILogger<EnqueueTicket> _log;

    public EnqueueTicket(QueueClient queue, ILogger<EnqueueTicket> log)
    {
        _queue = queue;
        _log = log;
    }

    [Function("EnqueueTicket")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "migrate/ticket/{sysId}")]
        HttpRequestData req,
        string sysId)
    {
        await _queue.CreateIfNotExistsAsync();

        var tracking = Guid.NewGuid().ToString("n");
        var payload = JsonSerializer.Serialize(new TicketMessage(sysId, tracking));

        await _queue.SendMessageAsync(payload);
        _log.LogInformation("Queued {SysId} tracking={Tracking}", sysId, tracking);

        var res = req.CreateResponse(HttpStatusCode.Accepted);
        await res.WriteStringAsync($"Queued {sysId} tracking={tracking}");
        return res;
    }
}
