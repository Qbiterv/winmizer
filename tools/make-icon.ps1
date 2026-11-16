<#
.SYNOPSIS
  Generates src\Winmizer\Assets\winmizer.ico (sizes 16-256) with System.Drawing.
  Motif: bold stylized "W" with a cyan -> purple gradient on a dark rounded tile.
.EXAMPLE
  .\tools\make-icon.ps1
#>
param(
    [string]$Output = (Join-Path $PSScriptRoot '..\src\Winmizer\Assets\winmizer.ico')
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$sizes = 16, 24, 32, 48, 64, 128, 256

function New-RoundedPath([single]$x, [single]$y, [single]$w, [single]$h, [single]$r) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $r * 2
    $p.AddArc($x, $y, $d, $d, 180, 90)
    $p.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $p.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $p.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $p.CloseFigure()
    return $p
}

function New-IconPng([int]$size) {
    # Draw at 4x and downsample for clean edges at small sizes.
    $s = $size * 4
    $bmp = New-Object System.Drawing.Bitmap $s, $s, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'
    $g.PixelOffsetMode = 'HighQuality'
    $g.Clear([System.Drawing.Color]::Transparent)

    # Dark tile with a subtle vertical gradient.
    $pad = $s * 0.03
    $tile = New-RoundedPath $pad $pad ($s - 2 * $pad) ($s - 2 * $pad) ($s * 0.22)
    $bg = New-Object System.Drawing.Drawing2D.LinearGradientBrush ([System.Drawing.PointF]::new(0, 0)), ([System.Drawing.PointF]::new(0, $s)),
        ([System.Drawing.Color]::FromArgb(255, 20, 26, 44)), ([System.Drawing.Color]::FromArgb(255, 8, 10, 20))
    $g.FillPath($bg, $tile)

    # Thin accent border.
    $edge = New-Object System.Drawing.Drawing2D.LinearGradientBrush ([System.Drawing.PointF]::new(0, 0)), ([System.Drawing.PointF]::new($s, $s)),
        ([System.Drawing.Color]::FromArgb(140, 0, 220, 255)), ([System.Drawing.Color]::FromArgb(140, 150, 80, 255))
    $edgePen = New-Object System.Drawing.Pen $edge, ([single]([Math]::Max(2, $s * 0.012)))
    $g.DrawPath($edgePen, $tile)

    # Stylized "W": two V strokes with a taller middle peak, speed-slanted slightly.
    $m = $s * 0.20
    $top = $s * 0.27; $bot = $s * 0.73; $mid = $s * 0.42
    $pts = [System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new($m, $top),
        [System.Drawing.PointF]::new($s * 0.335, $bot),
        [System.Drawing.PointF]::new($s * 0.50, $mid),
        [System.Drawing.PointF]::new($s * 0.665, $bot),
        [System.Drawing.PointF]::new($s - $m, $top)
    )
    $grad = New-Object System.Drawing.Drawing2D.LinearGradientBrush ([System.Drawing.PointF]::new($s * 0.12, 0)), ([System.Drawing.PointF]::new($s * 0.88, 0)),
        ([System.Drawing.Color]::FromArgb(255, 0, 229, 255)), ([System.Drawing.Color]::FromArgb(255, 168, 85, 247))
    $w = [single]($s * 0.115)

    # Soft glow under the W.
    $glow = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(45, 90, 140, 255)), ($w * 1.9)
    $glow.LineJoin = 'Round'; $glow.StartCap = 'Round'; $glow.EndCap = 'Round'
    $g.DrawLines($glow, $pts)

    $pen = New-Object System.Drawing.Pen $grad, $w
    $pen.LineJoin = 'Round'; $pen.StartCap = 'Round'; $pen.EndCap = 'Round'
    $g.DrawLines($pen, $pts)

    # Speed streaks on the left.
    $streak = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(150, 0, 229, 255)), ([single]($s * 0.028))
    $streak.StartCap = 'Round'; $streak.EndCap = 'Round'
    $g.DrawLine($streak, $s * 0.14, $s * 0.82, $s * 0.40, $s * 0.82)
    $g.DrawLine($streak, $s * 0.14, $s * 0.88, $s * 0.28, $s * 0.88)
    $g.Dispose()

    $out = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g2 = [System.Drawing.Graphics]::FromImage($out)
    $g2.InterpolationMode = 'HighQualityBicubic'
    $g2.PixelOffsetMode = 'HighQuality'
    $g2.SmoothingMode = 'HighQuality'
    $g2.DrawImage($bmp, 0, 0, $size, $size)
    $g2.Dispose(); $bmp.Dispose()

    $ms = New-Object System.IO.MemoryStream
    $out.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $out.Dispose()
    return , $ms.ToArray()
}

$images = foreach ($sz in $sizes) { , @($sz, (New-IconPng $sz)) }

# ICO container with PNG-compressed entries (supported since Windows Vista).
$dir = Split-Path -Parent $Output
New-Item -ItemType Directory -Force $dir | Out-Null
$fs = [System.IO.File]::Create((Resolve-Path $dir).Path + '\' + (Split-Path -Leaf $Output))
$bw = New-Object System.IO.BinaryWriter $fs
$bw.Write([uint16]0); $bw.Write([uint16]1); $bw.Write([uint16]$images.Count)
$offset = 6 + 16 * $images.Count
foreach ($img in $images) {
    $sz = $img[0]; $data = $img[1]
    $dim = if ($sz -ge 256) { 0 } else { $sz }
    $bw.Write([byte]$dim); $bw.Write([byte]$dim); $bw.Write([byte]0); $bw.Write([byte]0)
    $bw.Write([uint16]1); $bw.Write([uint16]32)
    $bw.Write([uint32]$data.Length); $bw.Write([uint32]$offset)
    $offset += $data.Length
}
foreach ($img in $images) { $bw.Write([byte[]]$img[1]) }
$bw.Dispose(); $fs.Dispose()

$info = Get-Item $Output
Write-Host "OK: $($info.FullName) ($($info.Length) bytes, sizes: $($sizes -join ', '))"


