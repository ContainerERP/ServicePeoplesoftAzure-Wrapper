 
namespace Psft.Shared
{
    public sealed class MigrationTicket
    {
        public required string Id { get; init; }
        public string? Number { get; init; }
        public ProjectMigrationRequest? Request { get; init; } 
        public string? Status { get; init; }                      // Queued, InProgress, Migrated, UnableToMigrate...
        public string? Reason { get; init; }                      // free text
        public DateTimeOffset? QueuedOnUtc { get; init; }
        public DateTimeOffset? MigratedOnUtc { get; init; }
      

    }
}