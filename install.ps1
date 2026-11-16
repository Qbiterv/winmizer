<#
.SYNOPSIS
  Installs Winmizer for the current user: copies the exe matching this machine's
  architecture to %LOCALAPPDATA%\Programs\Winmizer and creates Desktop + Start Menu shortcuts.
  It only copies a file and creates shortcuts - it never runs the optimizer.
.PARAMETER Uninstall
  Removes the shortcuts and the installed copy (Winmizer backups in %ProgramData% are kept).
.PARAMETER Source
  Path to a Winmizer.exe to install. Default: publish\<rid>\Winmizer.exe next to this script
  (or a Winmizer.exe placed next to this script, e.g. from a release zip).
.EXAMPLE
  .\install.ps1
  .\install.ps1 -Uninstall
#>
param(
    [switch]$Uninstall,
    [string]$Source
)

$ErrorActionPreference = 'Stop'

$installDir = Join-Path $env:LOCALAPPDATA 'Programs\Winmizer'
$installExe = Join-Path $installDir 'Winmizer.exe'
$desktopLnk = Join-Path ([Environment]::GetFolderPath('Desktop')) 'Winmizer.lnk'   # handles OneDrive-redirected desktops
$startLnk   = Join-Path ([Environment]::GetFolderPath('Programs')) 'Winmizer.lnk'  # per-user Start Menu\Programs

if ($Uninstall) {
    foreach ($f in $desktopLnk, $startLnk) {
        if (Test-Path $f) { Remove-Item $f -Force; Write-Host "Removed: $f" }
    }
    if (Test-Path $installDir) {
        try { Remove-Item $installDir -Recurse -Force; Write-Host "Removed: $installDir" }
        catch { Write-Warning "Could not remove $installDir (is Winmizer running?): $($_.Exception.Message)" }
    }
    Write-Host 'Winmizer uninstalled. Backups/logs in %ProgramData%\Winmizer were left untouched.' -ForegroundColor Green
    return
}

# --- pick the build that matches the machine architecture ---
$arch = try { [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString() }
        catch { if ($env:PROCESSOR_ARCHITEW6432) { $env:PROCESSOR_ARCHITEW6432 } else { $env:PROCESSOR_ARCHITECTURE } }
$rid = switch -Regex ($arch) {
    '^(Arm64|ARM64)$' { 'win-arm64' }
    '^(X86|x86)$'     { 'win-x86' }
    default           { 'win-x64' }
}

if (-not $Source) {
    $candidates = @(
        (Join-Path $PSScriptRoot "publish\$rid\Winmizer.exe"),
        (Join-Path $PSScriptRoot 'Winmizer.exe')
    )
    $Source = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
}
if (-not $Source -or -not (Test-Path $Source)) {
    throw "Winmizer.exe for $rid not found. Run .\build.ps1 first (expected publish\$rid\Winmizer.exe)."
}

Write-Host "==> Architecture: $arch -> $rid" -ForegroundColor Cyan
Write-Host "==> Installing $Source -> $installExe" -ForegroundColor Cyan
New-Item -ItemType Directory -Force $installDir | Out-Null
try { Copy-Item -LiteralPath $Source -Destination $installExe -Force }
catch { throw "Could not copy the exe (is Winmizer currently running? close it and retry): $($_.Exception.Message)" }

# --- shortcuts (the exe elevates itself via UAC for real runs; --list works without admin) ---
$shell = New-Object -ComObject WScript.Shell
foreach ($lnk in $desktopLnk, $startLnk) {
    New-Item -ItemType Directory -Force (Split-Path $lnk) | Out-Null
    $s = $shell.CreateShortcut($lnk)
    $s.TargetPath       = $installExe
    $s.WorkingDirectory = $installDir
    $s.IconLocation     = "$installExe,0"
    $s.Description      = 'Winmizer - optymalizator Windows 10/11 dla graczy'
    $s.Save()
    Write-Host "==> Shortcut: $lnk" -ForegroundColor Green
}
[void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($shell)

Write-Host 'Done. Launch Winmizer from the Desktop or Start Menu (UAC will ask for elevation).' -ForegroundColor Green
