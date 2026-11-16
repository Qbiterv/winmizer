<#
.SYNOPSIS
  Builds Winmizer and publishes a single self-contained Winmizer.exe (win-x64).
.EXAMPLE
  .\build.ps1
  .\build.ps1 -Output C:\temp\winmizer
#>
param(
    [string]$Configuration = 'Release',
    [string]$Output = (Join-Path $PSScriptRoot 'publish')
)

$ErrorActionPreference = 'Stop'

# Use the project-local tools if the SDK is not on PATH (non-admin install).
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    $tools = Join-Path $env:LOCALAPPDATA 'Winmizer-tools'
    if (Test-Path "$tools\dotnet\dotnet.exe") {
        $env:DOTNET_ROOT = "$tools\dotnet"
        $env:Path = "$tools\dotnet;$env:Path"
    }
    elseif (Test-Path 'C:\Program Files\dotnet\dotnet.exe') {
        $env:Path = "C:\Program Files\dotnet;$env:Path"
    }
    else { throw '.NET 8 SDK not found. Install it: winget install Microsoft.DotNet.SDK.8' }
}
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'

$project = Join-Path $PSScriptRoot 'src\Winmizer\Winmizer.csproj'

Write-Host '==> Publishing single-file win-x64 build...' -ForegroundColor Cyan
dotnet publish $project -c $Configuration -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -o $Output
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed ($LASTEXITCODE)" }

$exe = Join-Path $Output 'Winmizer.exe'
if (-not (Test-Path $exe)) { throw 'Winmizer.exe was not produced.' }
$size = [math]::Round((Get-Item $exe).Length / 1MB, 1)
Write-Host "==> OK: $exe ($size MB)" -ForegroundColor Green
Write-Host '    Run it as Administrator (it asks for UAC elevation itself; --list/--dry-run need no admin).'
