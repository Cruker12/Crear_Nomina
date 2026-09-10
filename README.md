# Generado Nomina System

Aplicación de escritorio Windows (.exe) para gestión de nóminas personalizadas,
cotizaciones y generación de documentos PDF/Excel.

## Stack

- **.NET 8 (LTS) + WPF + MVVM** + MaterialDesignInXAML
- **SQLite + EF Core 8** (local, sin servidor)
- **QuestPDF** (PDF) + **ClosedXML** (Excel)
- **xUnit + Moq + FluentAssertions** (testing)
- **Serilog** (logging) + **FluentValidation** (validación)

Moneda por defecto: **COP** (multimoneda soportada). Idioma: español.

## Arquitectura

Clean Architecture simplificada (4 capas):

```
Presentation -> Application -> Domain <- Infrastructure
```

Ver `docs/arquitectura.md`.

## Documentación

- `docs/arquitectura.md` — capas, responsabilidades, DI, persistencia, documentos.
- `docs/dominio.md` — entidades, value objects, enums, reglas, relaciones.
- `docs/decisiones.md` — stack y decisiones justificadas.
- `docs/roadmap.md` — 18 fases (F0–F17) con entregas y dependencias.
- `docs/release-v1.0.md` — notas del release: instalación, backup, troubleshooting, licencias.

## Estado

**v1.0.0 publicada** (tag `v1.0.0`). 138/138 tests. Ver `docs/release-v1.0.md`.

## Instalación (usuario final)

1. Copia la carpeta `win-x64` publicada a la máquina Windows 10/11 x64.
2. Ejecuta `GeneradoNominaSystem.Presentation.exe` (autocontenido, sin .NET previo).
3. Datos en `%LocalAppData%\GeneradoNominaSystem\`, documentos en `Mis Documentos\GeneradoNominaSystem`.

## Desarrollo

```powershell
dotnet build GeneradoNominaSystem.sln
dotnet test GeneradoNominaSystem.sln
powershell -ExecutionPolicy Bypass -File scripts/publish-windows.ps1
```

## Desarrollo por fases

No se avanza de fase sin cumplir criterios de aceptación.
Commits pequeños, atómicos, Conventional Commits.

```
F0 definición -> F1 init -> F2 base -> F3 dominio -> F4 persistencia
 -> F5 empresas -> F6 empleados -> F7 motor nómina -> F8 plantillas
 -> F9 documentos -> F10 cotizaciones -> F11 plantillas cotización
 -> F12 históricos -> F13 UX -> F14 testing -> F15 .exe -> F16 QA -> F17 release
```

## Requisitos (desarrollo)

- .NET 8 SDK (probado con 8.0.425)
- Visual Studio 2022 17.8+ o VS Code + C# Dev Kit
- Windows 10/11 x64
