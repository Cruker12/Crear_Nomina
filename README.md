# Generado Nomina System

Aplicación de escritorio para Windows (.exe) que permite gestionar empresas,
empleados, nóminas personalizadas, cotizaciones y generar documentos PDF/Excel,
todo con datos locales (sin servidor ni internet).

Versión actual: **v1.0.0** (tag `v1.0.0`). Ver `docs/release-v1.0.md`.

## Funcionalidades

- Empresas con datos fiscales y configuración (moneda por defecto COP, multimoneda en modelo).
- Empleados con validaciones e información laboral.
- Conceptos de nómina configurables (devengos, deducciones, beneficios, comisiones; fijos o porcentajes).
- Periodos de nómina con control anti-solape.
- Motor de cálculo determinista + flujo Borrador → Calculada → Aprobada → Pagada/Anulada.
- Plantillas de nómina reutilizables con valores por defecto.
- Productos/servicios + cotizaciones (líneas, descuentos, estados, numeración secuencial).
- Plantillas de cotización.
- Exportación a PDF y Excel + histórico inmutable de documentos generados.
- Históricos con filtros (texto, estado, fechas, tipo, formato).

Idioma: español.

## Requisitos

Para **usar** el programa:

- Windows 10/11 de 64 bits.
- No requiere instalar .NET (el paquete es autocontenido).
- Permisos de usuario normal.

Para **desarrollar**:

- .NET 8 SDK (probado con 8.0.425).
- Visual Studio 2022 17.8+ o VS Code + C# Dev Kit.
- Windows 10/11 x64.

## Instalación y ejecución

Opción recomendada (crea accesos directos en Escritorio y Menú Inicio):

```powershell
powershell -ExecutionPolicy Bypass -File scripts/instalar.ps1 -Compilar
```

O publica e instala en un solo paso:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/publish-windows.ps1 -Instalar
```

Instalación manual: copia la carpeta `publish/win-x64` a la máquina destino
y ejecuta `GeneradoNominaSystem.Presentation.exe` (autocontenido, sin .NET previo).

En el primer arranque se crean las carpetas y la base de datos automáticamente.
Datos en `%LocalAppData%\GeneradoNominaSystem\`,
documentos en `Mis Documentos\GeneradoNominaSystem`.
Para desinstalar: `scripts/desinstalar.ps1` (conserva los datos por defecto).

Si Windows SmartScreen advierte (exe sin firma), elige
"Más información → Ejecutar de todos modos".

## Guía rápida de uso

1. **Empresas**: crea tu empresa con NIT y datos fiscales.
2. **Empleados**: registra empleados vinculados a la empresa.
3. **Conceptos**: crea Salario (devengo), Salud/Pensión (deducción, 4 %).
4. **Periodos**: crea el periodo a liquidar (ej. Marzo 2026).
5. **Nóminas**: crea la nómina, agrega detalles, calcula (base = salario),
   aprueba y exporta a PDF/Excel.
6. **Plantillas**: guarda combinaciones de conceptos para reutilizarlas
   con "Desde plantilla".
7. **Productos / Cotizaciones**: arma el catálogo, crea cotizaciones con
   líneas y descuentos, cambia estados y expórtalas.
8. **Historial**: consulta nóminas, cotizaciones y documentos con filtros,
   y abre los archivos generados.

## Datos, backup y logs

- Base de datos: `%LocalAppData%\GeneradoNominaSystem\data\app.db`.
- Logs (rotación diaria, 30 días): `%LocalAppData%\GeneradoNominaSystem\logs\`.
- Documentos exportados: `Mis Documentos\GeneradoNominaSystem\`.
- **Backup**: con la app cerrada, copia `app.db` a un lugar seguro.
- **Restore**: con la app cerrada, devuelve el archivo a su ruta.

Los logs no contienen salarios ni documentos de identidad.

## Solución de problemas

| Síntoma | Acción |
|---|---|
| Diálogo "Error crítico" al abrir (config) | Re-copia la carpeta publicada (`appsettings.json` dañado) |
| Diálogo "Error crítico" de base de datos | Restaura el backup de `app.db` |
| Mensaje rojo en una vista | Es una validación de negocio: lee el mensaje y corrige el dato |
| No abre un documento del historial | El archivo fue movido/borrado: regenera desde su pestaña |

Detalle completo en `docs/release-v1.0.md`.

## Desarrollo

```powershell
dotnet build GeneradoNominaSystem.sln
dotnet test GeneradoNominaSystem.sln
powershell -ExecutionPolicy Bypass -File scripts/publish-windows.ps1
```

Estructura: `src/` (Domain, Application, Infrastructure, Presentation),
`tests/` (espejo por capa + integración), `docs/`, `scripts/`.

Convenciones: commits pequeños y atómicos con Conventional Commits
(`feat:`, `fix:`, `test:`, `refactor:`, `docs:`, `chore:`),
Clean Architecture (las dependencias apuntan al dominio),
sin lógica de negocio en la UI ni en los exportadores.

## Stack

- **.NET 8 (LTS) + WPF + MVVM** + MaterialDesignInXAML
- **SQLite + EF Core 8** (local, sin servidor)
- **QuestPDF** (PDF) + **ClosedXML** (Excel)
- **xUnit + Moq + FluentAssertions** (testing)
- **Serilog** (logging) + **FluentValidation** (validación)

## Documentación

- `docs/arquitectura.md` — capas, responsabilidades, DI, persistencia, documentos.
- `docs/dominio.md` — entidades, value objects, enums, reglas, relaciones.
- `docs/decisiones.md` — stack y decisiones justificadas.
- `docs/roadmap.md` — 18 fases (F0–F17) con entregas y dependencias.
- `docs/release-v1.0.md` — notas del release: instalación, backup, troubleshooting, licencias.

## Licencias de terceros

Runtime: QuestPDF (Community — gratis para individuos o empresas
< USD 1M/año; excluye sector público y cotizadas), ClosedXML, EF Core,
MaterialDesign, Serilog, FluentValidation y Microsoft.Extensions
(MIT / BSD-3 / Apache-2.0). Solo tests: xUnit, Moq, FluentAssertions, coverlet.
