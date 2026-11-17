<#
.SYNOPSIS
  Generates docs\social-preview.png (1280x640) - the GitHub social preview banner.
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File tools\make-social-preview.ps1
#>
param(
    [string]$Output = (Join-Path $PSScriptRoot '..\docs\social-preview.png')
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$W = 1280; $H = 640
function C([int]$a, [int]$r, [int]$g, [int]$b) { [System.Drawing.Color]::FromArgb($a, $r, $g, $b) }
function PF([double]$x, [double]$y) { [System.Drawing.PointF]::new([single]$x, [single]$y) }

function New-RoundedPath([double]$x, [double]$y, [double]$w, [double]$h, [double]$r) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $r * 2
    $p.AddArc([single]$x, [single]$y, [single]$d, [single]$d, 180, 90)
    $p.AddArc([single]($x + $w - $d), [single]$y, [single]$d, [single]$d, 270, 90)
    $p.AddArc([single]($x + $w - $d), [single]($y + $h - $d), [single]$d, [single]$d, 0, 90)
    $p.AddArc([single]$x, [single]($y + $h - $d), [single]$d, [single]$d, 90, 90)
    $p.CloseFigure()
    return $p
}

# Same drawing as tools\make-icon.ps1, rendered at 4x then downsampled.
function New-IconBitmap([int]$size) {
    $s = $size * 4
    $bmp = New-Object System.Drawing.Bitmap $s, $s, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'
    $g.PixelOffsetMode = 'HighQuality'
    $g.Clear([System.Drawing.Color]::Transparent)

    $pad = $s * 0.03
    $tile = New-RoundedPath $pad $pad ($s - 2 * $pad) ($s - 2 * $pad) ($s * 0.22)
    $bg = New-Object System.Drawing.Drawing2D.LinearGradientBrush (PF 0 0), (PF 0 $s), (C 255 20 26 44), (C 255 8 10 20)
    $g.FillPath($bg, $tile)
    $edge = New-Object System.Drawing.Drawing2D.LinearGradientBrush (PF 0 0), (PF $s $s), (C 140 0 220 255), (C 140 150 80 255)
    $g.DrawPath((New-Object System.Drawing.Pen $edge, ([single]([Math]::Max(2, $s * 0.012)))), $tile)

    $m = $s * 0.20
    $top = $s * 0.27; $bot = $s * 0.73; $mid = $s * 0.42
    $pts = [System.Drawing.PointF[]]@(
        (PF $m $top), (PF ($s * 0.335) $bot), (PF ($s * 0.50) $mid), (PF ($s * 0.665) $bot), (PF ($s - $m) $top))
    $grad = New-Object System.Drawing.Drawing2D.LinearGradientBrush (PF ($s * 0.12) 0), (PF ($s * 0.88) 0), (C 255 0 229 255), (C 255 168 85 247)
    $w = [single]($s * 0.115)
    $glow = New-Object System.Drawing.Pen (C 45 90 140 255), ($w * 1.9)
    $glow.LineJoin = 'Round'; $glow.StartCap = 'Round'; $glow.EndCap = 'Round'
    $g.DrawLines($glow, $pts)
    $pen = New-Object System.Drawing.Pen $grad, $w
    $pen.LineJoin = 'Round'; $pen.StartCap = 'Round'; $pen.EndCap = 'Round'
    $g.DrawLines($pen, $pts)
    $streak = New-Object System.Drawing.Pen (C 150 0 229 255), ([single]($s * 0.028))
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
    return $out
}

# Pick fonts that exist.
$installed = (New-Object System.Drawing.Text.InstalledFontCollection).Families | ForEach-Object { $_.Name }
function Pick-Font([string[]]$names) { foreach ($n in $names) { if ($installed -contains $n) { return $n } }; return 'Arial' }
$titleFont = Pick-Font @('Segoe UI Black', 'Bahnschrift', 'Segoe UI')
$bodyFont  = Pick-Font @('Segoe UI Semibold', 'Segoe UI', 'Arial')
$textFont  = Pick-Font @('Segoe UI', 'Arial')

$bmp = New-Object System.Drawing.Bitmap $W, $H, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = 'AntiAlias'
$g.InterpolationMode = 'HighQualityBicubic'
$g.PixelOffsetMode = 'HighQuality'
$g.TextRenderingHint = 'AntiAliasGridFit'

# --- Background gradient ---
$bg = New-Object System.Drawing.Drawing2D.LinearGradientBrush (PF 0 0), (PF $W $H), (C 255 14 19 36), (C 255 5 7 15)
$g.FillRectangle($bg, 0, 0, $W, $H)

# --- Soft glow blobs (radial via PathGradientBrush) ---
function Draw-Glow([double]$cx, [double]$cy, [double]$rx, [double]$ry, $color) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddEllipse([single]($cx - $rx), [single]($cy - $ry), [single](2 * $rx), [single](2 * $ry))
    $pb = New-Object System.Drawing.Drawing2D.PathGradientBrush $path
    $pb.CenterColor = $color
    $pb.SurroundColors = [System.Drawing.Color[]]@([System.Drawing.Color]::FromArgb(0, $color.R, $color.G, $color.B))
    $g.FillPath($pb, $path)
    $pb.Dispose(); $path.Dispose()
}
Draw-Glow 230 320 420 380 (C 70 0 200 255)
Draw-Glow 1100 80 480 300 (C 60 150 80 255)
Draw-Glow 900 640 420 220 (C 40 0 180 255)

# --- Faint grid + diagonal speed lines (clipped, very subtle) ---
$gridPen = New-Object System.Drawing.Pen (C 9 255 255 255), 1
for ($x = 0; $x -le $W; $x += 64) { $g.DrawLine($gridPen, $x, 0, $x, $H) }
for ($y = 0; $y -le $H; $y += 64) { $g.DrawLine($gridPen, 0, $y, $W, $y) }

$sp = New-Object System.Drawing.Pen (C 22 120 200 255), 2
$sp.StartCap = 'Round'; $sp.EndCap = 'Round'
$lines = @(@(700, 40, 300), @(860, 20, 200), @(1010, 60, 260), @(1120, 20, 160), @(-60, 560, 280), @(80, 610, 200))
foreach ($l in $lines) { $g.DrawLine($sp, [single]$l[0], [single]$l[1], [single]($l[0] + $l[2]), [single]($l[1] - $l[2] * 0.36)) }

# --- Icon (left) ---
$iconSize = 270
$ix = 100; $iy = ($H - $iconSize) / 2
Draw-Glow ($ix + $iconSize / 2) ($iy + $iconSize / 2) 260 260 (C 80 60 130 255)
$icon = New-IconBitmap $iconSize
$g.DrawImage($icon, [single]$ix, [single]$iy, [single]$iconSize, [single]$iconSize)
$icon.Dispose()

# --- Text (right) ---
$tx = 440
$sf = [System.Drawing.StringFormat]::GenericTypographic
$sf.FormatFlags = $sf.FormatFlags -bor [System.Drawing.StringFormatFlags]::NoWrap

$titleSize = 120
$tf = New-Object System.Drawing.Font $titleFont, $titleSize, ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Pixel)
$tm = $g.MeasureString('Winmizer', $tf, 2000, $sf)
$titleY = 118
$tgrad = New-Object System.Drawing.Drawing2D.LinearGradientBrush (PF $tx 0), (PF ($tx + $tm.Width) 0), (C 255 0 229 255), (C 255 168 85 247)
# soft shadow
$g.DrawString('Winmizer', $tf, (New-Object System.Drawing.SolidBrush (C 90 0 0 0)), [single]($tx + 3), [single]($titleY + 5), $sf)
$g.DrawString('Winmizer', $tf, $tgrad, [single]$tx, [single]$titleY, $sf)

# accent underline
$ulY = $titleY + $tm.Height + 8
$ulBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush (PF $tx 0), (PF ($tx + 120) 0), (C 255 0 229 255), (C 255 168 85 247)
$g.FillPath($ulBrush, (New-RoundedPath $tx $ulY 120 5 2.5))

# tagline
$tagF = New-Object System.Drawing.Font $bodyFont, 36, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
$tagY = $ulY + 26
$g.DrawString('Gaming optimizer for Windows 11 & 10', $tagF, (New-Object System.Drawing.SolidBrush (C 255 226 232 245)), [single]$tx, [single]$tagY, $sf)

# chips
$chipF = New-Object System.Drawing.Font $bodyFont, 21, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
$chips = 'Debloat', 'Power plans', 'Low latency', 'Safe & reversible'
$chipH = 46; $gap = 14; $padX = 20
$chipY = $tagY + 80
$cx = $tx
$i = 0
foreach ($c in $chips) {
    $m = $g.MeasureString($c, $chipF, 2000, $sf)
    $cw = $m.Width + 2 * $padX + 14
    $path = New-RoundedPath $cx $chipY $cw $chipH ($chipH / 2)
    $g.FillPath((New-Object System.Drawing.SolidBrush (C 28 160 190 255)), $path)
    $cg = New-Object System.Drawing.Drawing2D.LinearGradientBrush (PF $cx 0), (PF ($cx + $cw) 0), (C 150 0 220 255), (C 150 160 90 255)
    $g.DrawPath((New-Object System.Drawing.Pen $cg, 1.5), $path)
    # dot
    $dotB = New-Object System.Drawing.SolidBrush (C 255 0 229 255)
    $g.FillEllipse($dotB, [single]($cx + $padX - 2), [single]($chipY + $chipH / 2 - 4), 8, 8)
    $g.DrawString($c, $chipF, (New-Object System.Drawing.SolidBrush (C 255 214 224 242)), [single]($cx + $padX + 14), [single]($chipY + ($chipH - $m.Height) / 2), $sf)
    $cx += $cw + $gap
    $i++
}
$chipsRight = $cx - $gap

# footer
$ftF = New-Object System.Drawing.Font $textFont, 22, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
$g.DrawString('github.com/Qbiterv/winmizer', $ftF, (New-Object System.Drawing.SolidBrush (C 255 122 133 158)), [single]$tx, [single]($chipY + $chipH + 46), $sf)

$g.Dispose()
New-Item -ItemType Directory -Force (Split-Path -Parent $Output) | Out-Null
$full = Join-Path (Resolve-Path (Split-Path -Parent $Output)).Path (Split-Path -Leaf $Output)
$bmp.Save($full, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
$info = Get-Item $full
Write-Host "OK: $($info.FullName) ($([Math]::Round($info.Length / 1KB)) KB) fonts: $titleFont / $bodyFont; chips right edge x=$([int]$chipsRight)"
