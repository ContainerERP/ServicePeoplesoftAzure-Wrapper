# --- 02_push_images.ps1 -------------------------------------------------------
$ErrorActionPreference = "Stop"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
Start-Transcript -Path ".\02_push_images_${timestamp}.log" | Out-Null

# <<< EDIT THESE ONCE >>>
$rg   = "rg-snps-local"
$acr  = "snpsacr"

# Linux worker image (built locally)
$imgWorker = "psft-rest-worker"
$tagWorker = "iter3"

# Windows wrapper image (built locally)
$imgWrapper = "psftrestwrapper"
$tagWrapper = "sn-1"

# ACR details + login
$loginServer = $(az acr show -n $acr --query loginServer -o tsv).ToLower()
az acr login -n $acr | Out-Null

function Push-IfLocal {
  param([string]$local, [string]$remote)
  if ($(docker images -q $local)) {
    docker tag  $local  $remote
    docker push $remote
    Write-Host "Pushed $remote"
  } else {
    Write-Host "Local image $local not found—skip."
  }
}

# Worker
$localWorker  = "$($imgWorker):$($tagWorker)"
$remoteWorker = "$loginServer/$($imgWorker):$($tagWorker)"
Push-IfLocal -local $localWorker -remote $remoteWorker

# Wrapper (Windows)
$localWrapper  = "$($imgWrapper):$($tagWrapper)"
$remoteWrapper = "$loginServer/$($imgWrapper):$($tagWrapper)"
Push-IfLocal -local $localWrapper -remote $remoteWrapper

# Show repos (nice for article)
az acr repository list -n $acr -o table

Stop-Transcript | Out-Null
