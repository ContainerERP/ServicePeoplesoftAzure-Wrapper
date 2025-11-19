# --- 04_vnet_private.ps1 ------------------------------------------------------
$ErrorActionPreference = "Stop"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
Start-Transcript -Path ".\04_vnet_private_${timestamp}.log" | Out-Null

# <<< EDIT THESE ONCE >>>
$rg  = "rg-snps-local"
$loc = "canadacentral"

$vnetName = "snps-vnet"
$snetName = "cae-subnet"
$addrVnet = "10.50.0.0/16"
$addrSnet = "10.50.1.0/24"

# Create VNet + subnet if needed
if (-not $(az network vnet show -g $rg -n $vnetName 2>$null)) {
  az network vnet create -g $rg -n $vnetName -l $loc `
    --address-prefix $addrVnet `
    --subnet-name $snetName --subnet-prefix $addrSnet | Out-Null
  Write-Host "Created VNet $vnetName with subnet $snetName."
} else {
  Write-Host "VNet $vnetName exists."
}

$subnetId = $(az network vnet subnet show -g $rg --vnet-name $vnetName -n $snetName --query id -o tsv)

# Create a NEW Container Apps env attached to the subnet (keep old env for fallback)
$newEnv = "erp-env-vnet"
if (-not $(az containerapp env show -g $rg -n $newEnv 2>$null)) {
  az containerapp env create -g $rg -n $newEnv -l $loc `
    --infrastructure-subnet-resource-id $subnetId | Out-Null
  Write-Host "Created Container Apps env $newEnv (VNet-attached)."
} else {
  Write-Host "Container Apps env $newEnv already exists."
}

Write-Host "`nNext steps:"
Write-Host " - Re-deploy worker into $newEnv with '--ingress internal --target-port 80' if needed"
Write-Host " - Add private endpoints (Storage/Key Vault/PeopleSoft API) as required"
Stop-Transcript | Out-Null
