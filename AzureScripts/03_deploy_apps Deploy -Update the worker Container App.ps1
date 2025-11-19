# --- 03_deploy_apps.ps1 -------------------------------------------------------
$ErrorActionPreference = "Stop"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
Start-Transcript -Path ".\03_deploy_apps_${timestamp}.log" | Out-Null

# <<< EDIT THESE ONCE >>>
$rg        = "rg-snps-local"
$loc       = "canadacentral"
$acr       = "snpsacr"
$envName   = "erp-env"
$appWorker = "psft-worker-queue"

# Image reference pushed in 02
$imgWorker = "psft-rest-worker"
$tagWorker = "iter3"

# Storage / queue settings
$queueName = "psft-work-items"
# NOTE: keep simple for Iteration 3; move to Key Vault + Managed Identity later
$queueConn = "<YOUR_STORAGE_CONNECTION_STRING>"

# Resolve ACR loginServer
$loginServer = $(az acr show -n $acr --query loginServer -o tsv).ToLower()
$remoteWorker = "$loginServer/$($imgWorker):$($tagWorker)"

# Create or update the worker Container App
if (-not $(az containerapp show -g $rg -n $appWorker 2>$null)) {
  az containerapp create -g $rg -n $appWorker -e $envName `
    --image $remoteWorker `
    --registry-server $loginServer `
    --ingress disabled `
    --min-replicas 1 --max-replicas 2 `
    --env-vars AZURE_STORAGE_CONN="$queueConn" QUEUE_NAME="$queueName" | Out-Null
  Write-Host "Created Container App $appWorker."
} else {
  az containerapp update -g $rg -n $appWorker `
    --image $remoteWorker `
    --env-vars AZURE_STORAGE_CONN="$queueConn" QUEUE_NAME="$queueName" | Out-Null
  Write-Host "Updated Container App $appWorker."
}

# Quick status & logs
az containerapp revision list -g $rg -n $appWorker -o table
az containerapp logs show -g $rg -n $appWorker --type system --tail 50

Stop-Transcript | Out-Null
