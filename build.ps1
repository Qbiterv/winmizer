<#
.SYNOPSIS
  Builds Winmizer and publishes self-contained single-file Release builds for
  win-x64, win-arm64 and win-x86 into publish\<rid>\Winmizer.exe, plus
  publish\Winmizer-<version>-<rid>.zip archives and WiX v4 MSI installers
  (publish\Winmizer-<version>-<arch>.msi). Use -NoInstaller to skip the MSIs.
.EXAMPLE
  .\build.ps1
  .\build.ps1 -NoInstaller
  .\build.ps1 -Runtime win-x64
  .\build.ps1 -Output C:\temp\winmizer
#>
param(
    [string]$Configuration = 'Release',
    [string]$Output = (Join-Path $PSScriptRoot 'publish'),
    [string[]]$Runtime = @('win-x64', 'win-arm64', 'win-x86'),
    [switch]$NoInstaller
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

# Version comes from the csproj (<Version>).
$version = ([xml](Get-Content $project)).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
if (-not $version) { $version = '1.0.0' }

New-Item -ItemType Directory -Force $Output | Out-Null
$results = @()

foreach ($rid in $Runtime) {
    Write-Host "==> Publishing single-file $rid build (v$version)..." -ForegroundColor Cyan
    $dir = Join-Path $Output $rid
    if (Test-Path $dir) { Remove-Item $dir -Recurse -Force }

    dotnet publish $project -c $Configuration -r $rid --self-contained true `
        -p:PublishSingleFile=true -o $dir
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $rid ($LASTEXITCODE)" }

    $exe = Join-Path $dir 'Winmizer.exe'
    if (-not (Test-Path $exe)) { throw "Winmizer.exe was not produced for $rid." }

    # Drop debug symbols from the publish folder; the exe is the only deliverable.
    Get-ChildItem $dir -Filter *.pdb -ErrorAction SilentlyContinue | Remove-Item -Force

    $zip = Join-Path $Output "Winmizer-$version-$rid.zip"
    if (Test-Path $zip) { Remove-Item $zip -Force }
    Compress-Archive -Path $exe -DestinationPath $zip -CompressionLevel Optimal

    $msiMB = ''
    if (-not $NoInstaller) {
        # WiX Toolset v4 (WixToolset.Sdk 4.0.5, restored from NuGet) - one MSI per architecture.
        $arch = $rid -replace '^win-', ''
        Write-Host "==> Building MSI for $arch..." -ForegroundColor Cyan
        $wixproj = Join-Path $PSScriptRoot 'installer\Winmizer.Installer.wixproj'
        dotnet build $wixproj -c $Configuration -p:Platform=$arch -p:ProductVersion=$version -p:AppExe=$exe
        if ($LASTEXITCODE -ne 0) { throw "MSI build failed for $arch ($LASTEXITCODE)" }
        $built = Join-Path $PSScriptRoot "installer\bin\$arch\$Configuration\Winmizer-$version-$arch.msi"
        if (-not (Test-Path $built)) { throw "MSI was not produced for $arch." }
        $msi = Join-Path $Output "Winmizer-$version-$arch.msi"
        Copy-Item $built $msi -Force
        $msiMB = [math]::Round((Get-Item $msi).Length / 1MB, 1)
    }

    $results += [pscustomobject]@{
        MsiMB  = $msiMB
        RID    = $rid
        ExeMB  = [math]::Round((Get-Item $exe).Length / 1MB, 1)
        ZipMB  = [math]::Round((Get-Item $zip).Length / 1MB, 1)
        Exe    = $exe
    }
    Write-Host "==> OK: $exe" -ForegroundColor Green
}

Write-Host ''
$results | Format-Table RID, ExeMB, ZipMB, MsiMB, Exe -AutoSize
Write-Host '    Run it as Administrator (it asks for UAC elevation itself; --list/--dry-run need no admin).'
