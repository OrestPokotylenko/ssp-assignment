. "$PSScriptRoot/common.ps1"

function Install-WithWinget {
    param(
        [Parameter(Mandatory)]
        [string]$PackageId,

        [Parameter(Mandatory)]
        [string]$DisplayName
    )

    if (-not (Test-Command "winget")) {
        throw "$DisplayName is missing and winget is not available. Install it manually."
    }

    Write-Step "Installing $DisplayName"

    winget install `
        --id $PackageId `
        --exact `
        --accept-package-agreements `
        --accept-source-agreements

    Update-ProcessPath

    if ($LASTEXITCODE -ne 0) {
        Write-Host "winget returned exit code $LASTEXITCODE. Verifying installation..."
    }
}

function Add-DirectoryToUserPath {
    param(
        [Parameter(Mandatory)]
        [string]$Directory
    )

    if (-not (Test-Path $Directory)) {
        return
    }

    $UserPath = [System.Environment]::GetEnvironmentVariable(
        "Path",
        "User"
    )

    $PathEntries = @()

    if (-not [string]::IsNullOrWhiteSpace($UserPath)) {
        $PathEntries = $UserPath -split ";"
    }

    if ($PathEntries -notcontains $Directory) {
        Write-Host "Adding to User PATH: $Directory"

        $NewUserPath = if ([string]::IsNullOrWhiteSpace($UserPath)) {
            $Directory
        }
        else {
            "$UserPath;$Directory"
        }

        [System.Environment]::SetEnvironmentVariable(
            "Path",
            $NewUserPath,
            "User"
        )
    }

    if (($env:PATH -split ";") -notcontains $Directory) {
        $env:PATH += ";$Directory"
    }
}

function Initialize-AzureCli {
    if (Test-Command "az") {
        Write-Host "Azure CLI found."
        return
    }

    Install-WithWinget `
        -PackageId "Microsoft.AzureCLI" `
        -DisplayName "Azure CLI"

    Update-ProcessPath

    if (-not (Test-Command "az")) {
        throw "Azure CLI was installed but 'az' is still unavailable."
    }

    Write-Host "Azure CLI installed successfully."
}

function Initialize-DotNet10 {
    if (Test-Command "dotnet") {
        $Sdks = dotnet --list-sdks

        if ($Sdks -match '(?m)^10\.') {
            Write-Host ".NET 10 SDK found."
            return
        }
    }

    Install-WithWinget `
        -PackageId "Microsoft.DotNet.SDK.10" `
        -DisplayName ".NET 10 SDK"

    Update-ProcessPath

    if (-not (Test-Command "dotnet")) {
        throw ".NET SDK was installed but 'dotnet' is unavailable."
    }

    $Sdks = dotnet --list-sdks

    if ($Sdks -notmatch '(?m)^10\.') {
        throw ".NET 10 SDK is still unavailable after installation."
    }

    Write-Host ".NET 10 SDK installed successfully."
}

function Initialize-FunctionsCoreTools {
    if (Test-Command "func") {
        $Version = func --version

        if ($Version -match '^4\.') {
            Write-Host "Azure Functions Core Tools 4 found."
            return
        }

        throw "Azure Functions Core Tools version 4 is required. Current version: $Version"
    }

    $KnownPaths = @(
        "C:\Program Files\Microsoft\Azure Functions Core Tools",
        "$env:APPDATA\npm"
    )

    foreach ($Path in $KnownPaths) {
        if ([string]::IsNullOrWhiteSpace($Path)) {
            continue
        }

        $FuncExe = Join-Path $Path "func.exe"
        $FuncCmd = Join-Path $Path "func.cmd"

        if ((Test-Path $FuncExe) -or (Test-Path $FuncCmd)) {
            Write-Host "Azure Functions Core Tools found at:"
            Write-Host "  $Path"

            Add-DirectoryToUserPath -Directory $Path

            if (Test-Command "func") {
                $Version = func --version

                if ($Version -match '^4\.') {
                    Write-Host "Azure Functions Core Tools 4 added to PATH."
                    return
                }

                throw "Azure Functions Core Tools version 4 is required. Current version: $Version"
            }
        }
    }

    Install-WithWinget `
        -PackageId "Microsoft.Azure.FunctionsCoreTools" `
        -DisplayName "Azure Functions Core Tools"

    Update-ProcessPath

    $DefaultPath = "C:\Program Files\Microsoft\Azure Functions Core Tools"

    if (Test-Path (Join-Path $DefaultPath "func.exe")) {
        Add-DirectoryToUserPath -Directory $DefaultPath
    }

    if (-not (Test-Command "func")) {
        throw "Azure Functions Core Tools appears to be installed, but 'func' is not available in PATH."
    }

    $Version = func --version

    if ($Version -notmatch '^4\.') {
        throw "Azure Functions Core Tools version 4 is required. Current version: $Version"
    }

    Write-Host "Azure Functions Core Tools 4 installed successfully."
}

Write-Step "Checking deployment tools"

Initialize-AzureCli
Initialize-DotNet10
Initialize-FunctionsCoreTools

Write-Host ""
Write-Host "Required deployment tools are available."