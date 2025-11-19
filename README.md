## Iteration 3 — Azure + ServiceNow + PeopleSoft

The basic “ServicePeoplesoftAzure + HTTP Wrapper” flow is now extended to run **end-to-end in Azure**:

1. **Psft.Migrate.Functions** (`src/Psft.Migrate.Functions`)
   - Azure Function that receives migration requests (HTTP / Event Grid / queue).
   - Enqueues work items into the Storage Queue (`psft-work-items`).

2. **Queue Worker Container** (`src/Worker/PsftRestworker`)
   - Windows container running in Azure Container Apps.
   - Listens to the same Storage Queue.
   - Calls the ServiceNow REST API to create / update migration tickets.
   - Invokes the existing PeopleSoft wrapper (Docker) to run the actual migrate / compare / copy steps.

3. **Shared Library** (`src/Psft.Shared`)
   - Common contracts, logging helpers, and migration step orchestration used by both the Function and the Worker.
   - Keeps the flow testable and “folder-first”: each migration step is a small unit with Golden I/O tests. 
## Iteration 4 — VNet & Private Connectivity (planned)

Next step is to move the Function + Worker into an Azure Container Apps Environment
inside a VNet, so that:

- Calls to PeopleSoft (on-prem) go over a private VPN / ExpressRoute.
- ServiceNow and other SaaS integrations stay reachable over the public edge.
- The same queue-based pattern still drives all migrations; only the network path changes.
