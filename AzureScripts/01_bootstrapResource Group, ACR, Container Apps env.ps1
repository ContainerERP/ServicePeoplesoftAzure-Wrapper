# --- 01_bootstrap.ps1 ---------------------------------------------------------
$ErrorActionPreference = "Stop"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
Start-Transcript -Path ".\01_bootstrap_${timestamp}.log" | Out-Null

# <<< EDIT THESE ONCE >>>
$rg  = "rg-snps-local"
$loc = "canadacentral"
$acr = "snpsacr"
$envName = "erp-env"

# 1) Resource group
az group show -n $rg 2>$null | Out-Null
if ($LASTEXITCODE -ne 0) {
  az group create -n $rg -l $loc | Out-Null
  Write-Host "Created RG $rg ($loc)."
} else {
  Write-Host "RG $rg exists."
}

# 2) ACR
if (-not $(az acr show -n $acr -g $rg 2>$null)) {
  az acr create -g $rg -n $acr --sku Basic -l $loc | Out-Null
  Write-Host "Created ACR $acr."
} else {
  Write-Host "ACR $acr exists."
}

# 3) Container Apps env (no VNet yet—see 04)
az extension add -n containerapp --upgrade | Out-Null
if (-not $(az containerapp env show -g $rg -n $envName 2>$null)) {
  az containerapp env create -g $rg -n $envName -l $loc | Out-Null
  Write-Host "Created Container Apps env $envName."
} else {
  Write-Host "Container Apps env $envName exists."
} 

 NOTE about big images (e.g. PeopleSoft tools image)
# ---------------------------------------------------
# If you ALREADY have the image built locally (for example: psft-rest-3) and it is huge,
# do NOT rebuild it again. Just tag it and push it to ACR.
#
#   # Example (reuse existing local image):
#   $img = "psft-rest-3"
#   $tag = "iter3"
#
#   $loginServer = az acr show -n $acr --query loginServer -o tsv
#
#   # Tag existing local image for ACR
#   docker tag "$img:$tag" "$loginServer/$img:$tag"
#
#   # Push layers once to ACR (slow the first time, then cached)
#   docker push "$loginServer/$img:$tag"
#
# For very large images that already live in another registry (e.g. Docker Hub),
# it’s even better to import directly into ACR without pulling/pushing from your laptop:
#
#   az acr import `
#     -n $acr `
#     --source mydockerhubaccount/psft-rest-3:latest `
#     --image psft-rest-3:iter3
#
# This way you avoid rebuilding and also avoid re-uploading gigabytes of layers.
Stop-Transcript | Out-Null
