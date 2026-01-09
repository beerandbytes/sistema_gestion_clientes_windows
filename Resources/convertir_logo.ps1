# Script para convertir imagen a formato .ico
# Uso: .\convertir_logo.ps1 -ImagenPath "ruta\a\imagen.png"

param(
    [Parameter(Mandatory=$true)]
    [string]$ImagenPath
)

# Verificar que la imagen existe
if (-not (Test-Path $ImagenPath)) {
    Write-Host "Error: No se encontró la imagen en: $ImagenPath" -ForegroundColor Red
    exit 1
}

$outputPath = Join-Path $PSScriptRoot "logo.ico"

Write-Host "Convirtiendo imagen a formato .ico..." -ForegroundColor Yellow
Write-Host "Imagen origen: $ImagenPath" -ForegroundColor Cyan
Write-Host "Destino: $outputPath" -ForegroundColor Cyan

# Intentar usar ImageMagick si está disponible
$magick = Get-Command magick -ErrorAction SilentlyContinue
if ($magick) {
    Write-Host "Usando ImageMagick..." -ForegroundColor Green
    & magick $ImagenPath -define icon:auto-resize=256,128,96,64,48,32,16 $outputPath
    if ($LASTEXITCODE -eq 0) {
        Write-Host "¡Conversión exitosa!" -ForegroundColor Green
        Write-Host "El archivo logo.ico ha sido creado en: $outputPath" -ForegroundColor Green
        exit 0
    }
}

# Si ImageMagick no está disponible, usar .NET para crear un icono básico
Write-Host "ImageMagick no encontrado. Creando icono básico con .NET..." -ForegroundColor Yellow

Add-Type -AssemblyName System.Drawing

try {
    # Cargar la imagen original
    $originalImage = [System.Drawing.Image]::FromFile($ImagenPath)
    
    # Crear un bitmap de 256x256 (tamaño estándar para iconos)
    $iconSize = 256
    $bitmap = New-Object System.Drawing.Bitmap($iconSize, $iconSize)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    
    # Dibujar la imagen redimensionada
    $graphics.DrawImage($originalImage, 0, 0, $iconSize, $iconSize)
    
    # Guardar como .ico (usando un método alternativo)
    # Nota: .NET no tiene soporte nativo para .ico, así que guardamos como PNG temporal
    $tempPng = Join-Path $env:TEMP "logo_temp.png"
    $bitmap.Save($tempPng, [System.Drawing.Imaging.ImageFormat]::Png)
    
    Write-Host "Imagen redimensionada creada. Para convertir a .ico, usa una herramienta en línea:" -ForegroundColor Yellow
    Write-Host "1. Ve a https://convertio.co/es/png-ico/" -ForegroundColor Cyan
    Write-Host "2. Sube el archivo: $tempPng" -ForegroundColor Cyan
    Write-Host "3. Descarga el .ico y guárdalo como: $outputPath" -ForegroundColor Cyan
    
    # Limpiar
    $graphics.Dispose()
    $bitmap.Dispose()
    $originalImage.Dispose()
    
    Write-Host "`nO instala ImageMagick desde: https://imagemagick.org/script/download.php" -ForegroundColor Yellow
    
} catch {
    Write-Host "Error al procesar la imagen: $_" -ForegroundColor Red
    exit 1
}






