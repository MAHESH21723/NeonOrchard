$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$artPath = Join-Path $PSScriptRoot 'FeatureGraphic-1024x500.png'
$artBitmap = New-Object System.Drawing.Bitmap(1024, 500, ([System.Drawing.Imaging.PixelFormat]::Format24bppRgb))
$artGraphics = [System.Drawing.Graphics]::FromImage($artBitmap)
$artGraphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$artGraphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$artGraphics.Clear([System.Drawing.ColorTranslator]::FromHtml('#0A2229'))
$artWhite = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml('#F5F5E9'))
$artMuted = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml('#97B6B2'))
$artLime = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml('#C7F36B'))
$artCoral = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml('#FF8D77'))
$artInk = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml('#0A2229'))
$artDots = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml('#1C4045'))
$artTitle = New-Object System.Drawing.Font('Segoe UI', 78, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel))
$artSubtitle = New-Object System.Drawing.Font('Segoe UI', 23, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel))
$artLabel = New-Object System.Drawing.Font('Segoe UI', 16, ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Pixel))
try {
    for ($artY = 48; $artY -lt 468; $artY += 28) {
        for ($artX = 553; $artX -lt 985; $artX += 28) { $artGraphics.FillEllipse($artDots, $artX, $artY, 3, 3) }
    }
    $artGraphics.DrawString('N / O', $artLabel, $artLime, 61, 43)
    $artGraphics.DrawString('neon', $artTitle, $artWhite, 53, 97)
    $artGraphics.DrawString('orchard.', $artTitle, $artWhite, 53, 186)
    $artGraphics.DrawString('A classic, freshly grown.', $artSubtitle, $artMuted, 61, 308)
    $artGraphics.DrawString('24 LEVELS   /   ENDLESS PLAY   /   OFFLINE', $artLabel, $artLime, 62, 412)
    $artSnake = New-Object System.Drawing.Pen([System.Drawing.ColorTranslator]::FromHtml('#C7F36B'), 42)
    $artSnake.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $artSnake.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $artSnake.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $artPoints = [System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(855,143), [System.Drawing.PointF]::new(649,143),
        [System.Drawing.PointF]::new(629,163), [System.Drawing.PointF]::new(629,232),
        [System.Drawing.PointF]::new(649,252), [System.Drawing.PointF]::new(773,252),
        [System.Drawing.PointF]::new(793,272), [System.Drawing.PointF]::new(793,341),
        [System.Drawing.PointF]::new(773,361), [System.Drawing.PointF]::new(624,361))
    $artGraphics.DrawLines($artSnake, $artPoints)
    $artGraphics.FillEllipse($artInk, 851, 127, 9, 9)
    $artGraphics.FillEllipse($artInk, 851, 148, 9, 9)
    $artGraphics.FillEllipse($artCoral, 874, 269, 43, 43)
    $artGraphics.FillEllipse($artLime, 898, 258, 19, 10)
    $artGraphics.FillEllipse($artWhite, 884, 278, 6, 8)
    $artBitmap.Save($artPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $artSnake.Dispose()
} finally {
    $artGraphics.Dispose(); $artBitmap.Dispose()
    $artWhite.Dispose(); $artMuted.Dispose(); $artLime.Dispose(); $artCoral.Dispose(); $artInk.Dispose(); $artDots.Dispose()
    $artTitle.Dispose(); $artSubtitle.Dispose(); $artLabel.Dispose()
}
Write-Output $artPath
