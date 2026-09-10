#Requires -Version 5.1
<#
.SYNOPSIS
    Desinstala Generado Nomina System sin borrar los datos.
.DESCRIPTION
    Elimina la carpeta de la aplicacion y los accesos directos del
    Escritorio y el Menu Inicio. Por defecto CONSERVA la base de datos
    y los logs en %LocalAppData%\GeneradoNominaSystem. Usa -BorrarDatos
    solo si quieres eliminar todo (con la app cerrada).
.EXAMPLE
    .\scripts\desinstalar.ps1
.EXAMPLE
    .\scripts\desinstalar.ps1 -Destino "C:\Programas\GeneradoNominaSystem" -BorrarDatos
#>
[CmdletBinding()]
param(
    [string]$Destino = (Join-Path ([Environment]::GetFolderPath("LocalApplicationData")) "GeneradoNominaSystem\app"),
    [switch]$BorrarDatos
)

$ErrorActionPreference = "Stop"

$nombreApp = "Generado Nomina System"

foreach ($lnk in @(
    (Join-Path ([Environment]::GetFolderPath("Desktop")) "$nombreApp.lnk"),
    (Join-Path ([Environment]::GetFolderPath("Programs")) "$nombreApp.lnk")
)) {
    if (Test-Path -LiteralPath $lnk) {
        Remove-Item -LiteralPath $lnk -Force
        Write-Host "OK: acceso eliminado $lnk" -ForegroundColor Green
    }
}

if (Test-Path -LiteralPath $Destino) {
    Remove-Item -LiteralPath $Destino -Recurse -Force
    Write-Host "OK: carpeta eliminada $Destino" -ForegroundColor Green
}
else {
    Write-Host "No existe $Destino, nada que eliminar." -ForegroundColor Yellow
}

$datos = Join-Path ([Environment]::GetFolderPath("LocalApplicationData")) "GeneradoNominaSystem"
if ($BorrarDatos) {
    if (Test-Path -LiteralPath $datos) {
        Remove-Item -LiteralPath $datos -Recurse -Force
        Write-Host "OK: datos eliminados $datos" -ForegroundColor Green
    }
}
else {
    Write-Host "Datos conservados en $datos (usa -BorrarDatos para eliminarlos)." -ForegroundColor Cyan
}

Write-Host "Desinstalacion completa." -ForegroundColor Green
