namespace   Psft.Shared
{
    // Domain payload the orchestrator already understands
    /* public sealed record ProjectMigrationRequest(
        string Project,
        string SourceDb,
        string TargetDb,
        string SourceUser,
        string SourcePwd,
        string TargetUser,
        string TargetPwd,
        string ConnectId,
        string ConnectPwd);

    */
    public sealed class ProjectMigrationRequest
    {
        // Core routing
        public string Project { get; set; } = string.Empty;
        public string SourceDb { get; set; } = string.Empty;
        public string TargetDb { get; set; } = string.Empty;

        // PeopleSoft users (app/user credentials)
        public string SourceUser { get; set; } = string.Empty;
        public string SourcePwd { get; set; } = string.Empty;
        public string TargetUser { get; set; } = string.Empty;
        public string TargetPwd { get; set; } = string.Empty;

        // DB connect (two-task/dblink/oracle account used by tools)
        public string ConnectId { get; set; } = string.Empty;
        public string ConnectPwd { get; set; } = string.Empty;
        public string DbUser { get; set; } = string.Empty;
        public string DbPwd { get; set; } = string.Empty; 
        public string? WorkDir { get; set; }
        public bool? ExportForUndo { get; set; }
    }
    // Ticket abstraction the wrapper uses
    /* public sealed record MigrationTicket(
        string Id,                      // e.g., ServiceNow sys_id
        string Number,                  // human-friendly
        ProjectMigrationRequest Request // everything Orchestrator needs
    );
    */

    /*
    // Read queued work
    public interface ITicketSource
    {
        Task<IReadOnlyList<MigrationTicket>> GetQueuedAsync(CancellationToken ct);
        Task<MigrationTicket?> GetAsync(string id, CancellationToken ct);
    }
   
    // Report progress/results
    public interface ITicketSink
    {
        Task UpdateStatusAsync(
            string id,
            string status,              // "InProgress","Migrated","UnableToMigrate"
            string reason,              // "Running Compare...", "Compare failed", etc.
            DateTime? queuedOn,
            DateTime? migratedOn,
            CancellationToken ct);
    }  */
}