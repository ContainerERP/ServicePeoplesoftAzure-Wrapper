# PeopleSoft Migration – .NET Wrapper

Thin ASP.NET Core API that **orchestrates PeopleSoft tools** (`pside.exe`, `sqlplus.exe`) using a simple HTTP interface.  
Part of a 3-step series:

- **Step 1 (Container & tools):** https://github.com/ContainerERP/ServicePeoplesoftAzure  
- **Step 2 (This repo – Wrapper API):** call migration steps over HTTP  
- **Step 3 (Next):** Azure Function / ServiceNow integration that calls this API

---

## What it does

- Exposes endpoints like `POST /api/migrate/step/{name}` (`Compare`, `CopyProject`, `Build`, `EmptyContainer`).
- Runs PeopleSoft CLI with templated arguments.
- Polls DB readiness with a per-step SQL probe.
- Streams back result JSON: `ok`, `ms`, `stdout`, `stderr`, `exitCode`.

---

## Prerequisites

- Windows host (or Windows container)
- .NET 9 SDK (for dev) / .NET runtime (if not self-contained)
- Access to Oracle DB used by your PeopleSoft env
- If running tools **inside this app**: required Oracle client DLLs on `PATH` and PeopleSoft tools available on disk  
  *(In containers, these are provided by Step 1)*

---

## Quick start

```bash
# 1) clone
git clone https://github.com/ContainerERP/ServicePeoplesoftAzure-Wrapper.git
cd ServicePeoplesoftAzure-Wrapper

# 2) configure
#   - appsettings.Development.json for local dev
#   - appsettings.Container.json for container runs
# See sample config below.

# 3) run (dev)
dotnet run --project PsftApi/PsftRestWrapper

# or publish self-contained (recommended for servers/containers)
dotnet publish PsftApi/PsftRestWrapper -c Release -r win-x64 --self-contained true
# binary will be under: PsftApi/PsftRestWrapper/bin/Release/net9.0/win-x64


# ServicePeoplesoftAzure-Wrapper
**Step 2** of my PeopleSoft migration series — a small **.NET HTTP wrapper** that exposes PeopleSoft tools
(Compare / Copy / Build) as REST endpoints. Use it from Postman, ServiceNow, or Azure Functions.

- **Step 1 (Tools Container):** https://github.com/ContainerERP/ServicePeoplesoftAzure
- **Step 2 (This wrapper):** https://github.com/ContainerERP/ServicePeoplesoftAzure-Wrapper
 
Call an endpoint (example)

PowerShell

$body = @{
  project   = "ISA_TEST2A"
  sourceDb  = "DEVL"
  targetDb  = "FSUAT"
  dbUser    = "user"
  dbPwd     = "pwd"
  connectId = "people"
  connectPwd= "pooo0ple"
} | ConvertTo-Json

curl -X POST http://localhost:60075/api/migrate/step/Compare `
  -H "Content-Type: application/json" -d $body


curl (bash)

curl -X POST http://localhost:60075/api/migrate/step/Compare \
  -H "Content-Type: application/json" \
  -d '{
{
  "project": "ISA_TEST2A",
  "sourceServer": "",
  "sourceDb": "DEVL",
  "sourceUser": "user",
  "sourcePwd": "password",
  "targetServer": "",
  "targetDb": "FSTST",
  "targetUser": "user",
  "targetPwd": "password",
  "connectId": "people",
  "connectPwd": "conpwd",
  "workDir": "C:\\temp\\export",
  "dbUser":   "dbuser",
  "dbPwd":    "dbpwd" ,  
  "exportForUndo": false 
}
  }'

Configuration

The wrapper reads standard ASP.NET config (appsettings.json + environment-specific files).
Key bits you’ll likely touch:

{
  "Psoft": {
    "PsidePath": "C:\\tools\\psft_portable\\pside.exe",
    "ArgsManifestPath": "C:\\app\\cfg\\pside-args.json",
    "ReportDir": "C:\\app\\data\\reports",
    "ExportDir": "C:\\app\\data\\export",
    "BuildCfgPath": "C:\\app\\cfg\\ptbld.cfg"
  },
  "Sql": {
    "SqlPlusPath": "C:\\OracleClient\\sqlplus.exe"
  },
  "StepChecks": {
    "TimeoutMs": 600000,
    "ProbeMs": 20000,
    "PollMs": 5000,
    "ByStep": {
      "CopyProject": { "dbRole": "source", "sql": "/* probe sql */" },
      "Compare":     { "dbRole": "source", "sql": "/* probe sql */" },
      "Build":       { "dbRole": "target", "sql": "/* probe sql */" }
    }
  }
}


For containers, we ship appsettings.container.json.
Select it with ASPNETCORE_ENVIRONMENT=Container.

Troubleshooting

PSORA64 / OCI / Oracle client DLL errors
Ensure the 64-bit client is present and on PATH. At minimum, oci.dll, oraociei19.dll, oraons.dll must be
findable. In the container, we place them under C:\OracleClient and set PATH accordingly.

“Cannot access path” / no logs
Pre-create and grant modify to the working dirs (e.g., C:\app\data\reports, C:\app\data\temp) if you run with a
locked-down user.

“Command 'build' not found in manifest”
Step names must match your manifest/args (e.g., CopyProject, Compare, Build). Casing matters for the keys in
StepChecks.ByStep.

What’s next (Step 3)

Azure Function that listens to ServiceNow and calls this wrapper.
Stay tuned.

License / Issues

License: MIT (or your choice)

Problems or ideas? Open an issue in this repo.
 
