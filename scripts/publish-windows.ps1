#Requires -Version 5.1
<#
.SYNOPSIS
    Publica Generado Nomina System como .exe autocontenido para Windows x64.
.DESCRIPTION
    Ejecuta dotnet publish en Release, self-contained, runtime win-x64.
    La salida queda en publish/win-x64/ (ignorada por Git).
    No requiere SDK ni runtime en la máquina destino.
.EXAMPLE
    .\scripts\publish-windows.ps1
#>
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$raiz = Split-Path -Parent $PSScriptRoot
$salida = Join-Path $raiz "publish\win-x64"

Write-Host "Publicando Generado Nomina System (Release, win-x64, self-contained)..." -ForegroundColor Cyan

dotnet publish "$raiz\src\GeneradoNominaSystem.Presentation\GeneradoNominaSystem.Presentation.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -o $salida

$exe = Join-Path $salida "GeneradoNominaSystem.Presentation.exe"
if (-not (Test-Path -LiteralPath $exe)) {
    throw "No se generó el ejecutable esperado: $exe"
}

$tamanoMb = [math]::Round((Get-Item -LiteralPath $exe).Length / 1MB, 1)
Write-Host "OK: $exe ($tamanoMb MB)" -ForegroundColor Green
Write-Host "Salida completa: $salida"
Write-Host "Para distribuir, copia la carpeta win-x64 completa (xcopy-deploy)."
