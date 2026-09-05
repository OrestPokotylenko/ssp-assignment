param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("dev", "prod")]
    [string]$Environment = "dev"
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/common.ps1"

$RepositoryRoot = Split-Path $PSScriptRoot -Parent

$Location = "francecentral"

$Solution = Join-Path $RepositoryRoot "WeatherPix.slnx"
$FunctionProject = Join-Path $RepositoryRoot "src/WeatherPix.Functions/WeatherPix.Functions.csproj"
$ParameterFile = Join-Path $RepositoryRoot "infra/parameters/$Environment.bicepparam"

$PexelsSecretName = "pexels-api-key"

Write-Host ""
Write-Host "WeatherPix deployment"
Write-Host "Environment: $Environment"

# --------------------------------------------------
# Tools
# --------------------------------------------------

Write-Step "Checking required tools"

& "$PSScriptRoot/setup-tools.ps1"

# --------------------------------------------------
# Validate repository
# --------------------------------------------------

Write-Step "Validating repository files"

Assert-FileExists $Solution
Assert-FileExists $FunctionProject
Assert-FileExists $ParameterFile

# --------------------------------------------------
# Azure authentication
# --------------------------------------------------

Assert-AzureLogin

# --------------------------------------------------
# Validate local secrets
# --------------------------------------------------

Write-Step "Checking deployment secrets"

Assert-EnvironmentVariable "PEXELS_API_KEY"

# --------------------------------------------------
# Restore
# --------------------------------------------------

Write-Step "Restoring solution"

dotnet restore $Solution

Assert-LastExitCode "dotnet restore failed."

# --------------------------------------------------
# Build
# --------------------------------------------------

Write-Step "Building solution"

dotnet build `
    $Solution `
    --configuration Release `
    --no-restore

Assert-LastExitCode "dotnet build failed."

# --------------------------------------------------
# Infrastructure what-if
# --------------------------------------------------

Write-Step "Running Azure infrastructure what-if"

az deployment sub what-if `
    --location $Location `
    --parameters $ParameterFile

Assert-LastExitCode "Infrastructure what-if failed."

# --------------------------------------------------
# Infrastructure deployment
# --------------------------------------------------

$Timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$DeploymentName = "weatherpix-$Environment-$Timestamp"

Write-Step "Deploying Azure infrastructure"

$DeploymentJson = az deployment sub create `
    --name $DeploymentName `
    --location $Location `
    --parameters $ParameterFile `
    --output json

Assert-LastExitCode "Infrastructure deployment failed."

$Deployment = $DeploymentJson | ConvertFrom-Json

$ResourceGroup = $Deployment.properties.outputs.resourceGroupName.value
$FunctionApp = $Deployment.properties.outputs.functionAppName.value
$KeyVault = $Deployment.properties.outputs.keyVaultName.value

Write-Host ""
Write-Host "Resource Group: $ResourceGroup"
Write-Host "Function App:   $FunctionApp"
Write-Host "Key Vault:      $KeyVault"

# --------------------------------------------------
# Configure local deployer Key Vault access
# --------------------------------------------------

Write-Step "Configuring local deployment access to Key Vault"

$CurrentUserObjectId = az ad signed-in-user show `
    --query id `
    --output tsv

Assert-LastExitCode "Could not determine current Azure user."

if ([string]::IsNullOrWhiteSpace($CurrentUserObjectId)) {
    throw "Could not determine current Azure user object ID."
}

$KeyVaultId = az keyvault show `
    --name $KeyVault `
    --resource-group $ResourceGroup `
    --query id `
    --output tsv

Assert-LastExitCode "Could not retrieve Key Vault resource ID."

$ExistingAssignment = az role assignment list `
    --assignee $CurrentUserObjectId `
    --scope $KeyVaultId `
    --role "Key Vault Secrets Officer" `
    --query "[0].id" `
    --output tsv

if ([string]::IsNullOrWhiteSpace($ExistingAssignment)) {
    Write-Host "Granting Key Vault Secrets Officer to current user..."

    az role assignment create `
        --assignee-object-id $CurrentUserObjectId `
        --assignee-principal-type User `
        --role "Key Vault Secrets Officer" `
        --scope $KeyVaultId `
        --output none

    Assert-LastExitCode "Failed to assign Key Vault Secrets Officer role."

    Write-Host "Waiting for RBAC propagation..."
    Start-Sleep -Seconds 15
}
else {
    Write-Host "Key Vault Secrets Officer role already assigned."
}

# --------------------------------------------------
# Store Pexels secret
# --------------------------------------------------

Write-Step "Storing Pexels API key in Key Vault"

$SecretStored = $false

for ($Attempt = 1; $Attempt -le 6; $Attempt++) {
    Write-Host "Attempt $Attempt of 6..."

    az keyvault secret set `
        --vault-name $KeyVault `
        --name $PexelsSecretName `
        --value $env:PEXELS_API_KEY `
        --output none

    if ($LASTEXITCODE -eq 0) {
        $SecretStored = $true
        break
    }

    if ($Attempt -lt 6) {
        Write-Host "Waiting for RBAC propagation..."
        Start-Sleep -Seconds 10
    }
}

if (-not $SecretStored) {
    throw "Failed to store Pexels API key in Key Vault."
}

# --------------------------------------------------
# Function App deployment
# --------------------------------------------------

Write-Step "Deploying Azure Function App"

Push-Location (Split-Path $FunctionProject -Parent)

try {
    func azure functionapp publish $FunctionApp

    Assert-LastExitCode "Function App deployment failed."
}
finally {
    Pop-Location
}

# --------------------------------------------------
# Result
# --------------------------------------------------

$Hostname = az functionapp show `
    --resource-group $ResourceGroup `
    --name $FunctionApp `
    --query "defaultHostName" `
    --output tsv

Assert-LastExitCode "Could not retrieve Function App hostname."

Write-Host ""
Write-Host "========================================="
Write-Host "WeatherPix deployed successfully."
Write-Host "Environment: $Environment"
Write-Host "Endpoint:    https://$Hostname"
Write-Host "========================================="