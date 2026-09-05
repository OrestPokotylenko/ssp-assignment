$ErrorActionPreference = "Stop"

function Write-Step {
    param(
        [Parameter(Mandatory)]
        [string]$Message
    )

    Write-Host ""
    Write-Host "==> $Message"
}

function Update-ProcessPath {
    $MachinePath = [System.Environment]::GetEnvironmentVariable("PATH", "Machine")
    $UserPath = [System.Environment]::GetEnvironmentVariable("PATH", "User")

    $env:PATH = "$MachinePath;$UserPath"
}

function Test-Command {
    param(
        [Parameter(Mandatory)]
        [string]$Command
    )

    return $null -ne (Get-Command $Command -ErrorAction SilentlyContinue)
}

function Assert-LastExitCode {
    param(
        [Parameter(Mandatory)]
        [string]$Message
    )

    if ($LASTEXITCODE -ne 0) {
        throw $Message
    }
}

function Assert-FileExists {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    if (-not (Test-Path $Path)) {
        throw "Required file was not found: $Path"
    }
}

function Assert-EnvironmentVariable {
    param(
        [Parameter(Mandatory)]
        [string]$Name
    )

    $Value = [System.Environment]::GetEnvironmentVariable($Name)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        throw "Required environment variable '$Name' is missing."
    }
}

function Assert-AzureLogin {
    Write-Step "Checking Azure login"

    $AccountJson = az account show --output json 2>$null

    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($AccountJson)) {
        Write-Host "No active Azure CLI session found."
        Write-Host "Opening Azure login..."

        az login
        Assert-LastExitCode "Azure login failed."

        $AccountJson = az account show --output json
        Assert-LastExitCode "Could not read Azure account information."
    }

    $Account = $AccountJson | ConvertFrom-Json

    Write-Host "Subscription: $($Account.name)"
    Write-Host "Subscription ID: $($Account.id)"

    return $Account
}