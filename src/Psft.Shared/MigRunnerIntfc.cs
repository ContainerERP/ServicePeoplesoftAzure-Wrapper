
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Psft.Shared;

public interface ITicketSource
{
   Task<IReadOnlyList<MigrationTicket>> GetQueuedAsync(CancellationToken ct);
     Task<MigrationTicket?> GetAsync(string id, CancellationToken ct);


}

public interface ITicketSink
{
    Task UpdateStatusAsync(
        string id,
        string status,                  // "Queued" | "InProgress" | "Migrated" | "UnableToMigrate" | ...
        string? reason,                 // message like "Running Compare..." or failure reason
        DateTimeOffset? queuedOnUtc,
        DateTimeOffset? migratedOnUtc,
        CancellationToken ct);
}
