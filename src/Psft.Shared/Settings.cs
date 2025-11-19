namespace Psft.Shared
{
    public sealed class PsoftSettings
    {
        public string PsidePath { get; set; } = @"psft_portable\pside.exe";
        public string SqlPlusPath { get; set; } = @"OracleClient\sqlplus.exe";
        public string TempDir { get; set; } = @"data\temp";
        public string ReportDir { get; set; } = @"data\reports";
        public string ExportDir { get; set; } = @"data\export";
        public string BuildCfgPath { get; set; } = @"data\ptbld.cfg";
        public string ArgsManifestPath { get; set; } = @"cfg\pside-args.json";
        public int TimeoutMs { get; set; } = 900000;
        public string SqlCheckScript { get; set; } = string.Empty;

    }


    public sealed class Settings
    {

        public Defaults Defaults { get; init; } = new();
    }
    public sealed class Defaults
    {
        public string SourceUser { get; init; } = "";
        public string SourcePwd { get; init; } = "";
        public string TargetUser { get; init; } = "";
        public string TargetPwd { get; init; } = "";
        public string ConnectId { get; init; } = "";
        public string ConnectPwd { get; init; } = "";

        public string DbUser { get; init; } = "";
        public string DbPwd { get; init; } = "";
    }
}