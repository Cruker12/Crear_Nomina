#Requires -Version 5.1
<#
.SYNOPSIS
    Instala Generado Nomina System: copia el .exe y crea accesos directos.
.DESCRIPTION
    Copia publish\win-x64 a la carpeta destino (por defecto
    %LocalAppData%\GeneradoNominaSystem\app, sin permisos de admin)
    y crea un acceso directo en el Escritorio y en el Menu Inicio.
    Los datos (app.db, logs) viven aparte y no se tocan.
    Si no existe la carpeta publish\win-x64, usa -Compilar para generarla primero.
.EXAMPLE
    .\scripts\instalar.ps1
.EXAMPLE
    .\scripts\instalar.ps1 -Destino "C:\Programas\GeneradoNominaSystem" -Compilar
#>
[CmdletBinding()]
param(
    [string]$Destino = (Join-Path ([Environment]::GetFolderPath("LocalApplicationData")) "GeneradoNominaSystem\app"),
    [switch]$Compilar,
    [switch]$SinAccesosDirectos
)

$ErrorActionPreference = "Stop"

$raiz = Split-Path -Parent $PSScriptRoot
$origen = Join-Path $raiz "publish\win-x64"
$nombreExe = "GeneradoNominaSystem.Presentation.exe"
$nombreApp = "Generado Nomina System"

if ($Compilar) {
    & (Join-Path $PSScriptRoot "publish-windows.ps1")
}

$exeOrigen = Join-Path $origen $nombreExe
if (-not (Test-Path -LiteralPath $exeOrigen)) {
    throw "No existe $exeOrigen. Ejecuta scripts\publish-windows.ps1 primero o usa -Compilar."
}

Write-Host "Instalando $nombreApp en $Destino ..." -ForegroundColor Cyan
New-Item -ItemType Directory -Path $Destino -Force | Out-Null
Copy-Item -Path (Join-Path $origen "*") -Destination $Destino -Recurse -Force

$exeDestino = Join-Path $Destino $nombreExe
if (-not (Test-Path -LiteralPath $exeDestino)) {
    throw "La copia fallo: no existe $exeDestino."
}
Write-Host "OK: archivos copiados." -ForegroundColor Green

if (-not $SinAccesosDirectos) {
    $wshell = New-Object -ComObject WScript.Shell
    $accesos = @(
        (Join-Path ([Environment]::GetFolderPath("Desktop")) "$nombreApp.lnk"),
        (Join-Path ([Environment]::GetFolderPath("Programs")) "$nombreApp.lnk")
    )
    foreach ($lnk in $accesos) {
        $acceso = $wshell.CreateShortcut($lnk)
        $acceso.TargetPath = $exeDestino
        $acceso.WorkingDirectory = $Destino
        $acceso.IconLocation = $exeDestino
        $acceso.Description = "$nombreApp - gestion de nominas y cotizaciones"
        $acceso.Save()
        Write-Host "OK: acceso directo $lnk" -ForegroundColor Green
    }
}

$datos = Join-Path ([Environment]::GetFolderPath("LocalApplicationData")) "GeneradoNominaSystem\data\app.db"
Write-Host ""
Write-Host "Instalacion completa. Abre '$nombreApp' desde el Escritorio o el Menu Inicio." -ForegroundColor Green
Write-Host "Datos: $datos"
