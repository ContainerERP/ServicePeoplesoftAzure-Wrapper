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
