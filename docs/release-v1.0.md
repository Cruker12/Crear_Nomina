# Release v1.0.0 — Generado Nomina System

Fecha: 2026-09-10
Tag: `v1.0.0`
Estado: ✅ Verificado (138/138 tests + smoke del .exe desde cero sin errores)

## Contenido

- Empresas (CRUD, NIT único, baja lógica) y configuración por empresa.
- Empleados (CRUD, unicidad por empresa+documento, baja lógica).
- Conceptos de nómina (devengo, deducción, beneficio, comisión; fijos y porcentajes).
- Periodos de nómina con control anti-solape.
- Motor de cálculo determinista + flujo Borrador → Calculada → Aprobada → Pagada/Anulada.
- Plantillas de nómina reutilizables con valores por defecto.
- Exportación de nóminas a PDF y Excel + histórico inmutable de documentos.
- Productos/servicios + cotizaciones (líneas, descuentos, estados, numeración).
- Plantillas de cotización + exportación a PDF/Excel.
- Históricos con filtros (texto, estado, fechas, tipo, formato) y apertura de archivos.
- UI Material Design en español, moneda por defecto COP (multimoneda en modelo).

## Requisitos (máquina destino)

- Windows 10/11 x64.
- Sin .NET instalado: el paquete es autocontenido (~211 MB).
- Permisos de usuario normal (todo vive en `%LocalAppData%` y `Mis Documentos`).

## Instalación

Opción recomendada (accesos directos en Escritorio y Menú Inicio):

```powershell
powershell -ExecutionPolicy Bypass -File scripts/instalar.ps1 -Compilar
```

Instalación manual:

1. Copia la carpeta `win-x64` del paquete a la máquina destino
   (ej. `C:\Programas\GeneradoNominaSystem`).
2. Ejecuta `GeneradoNominaSystem.Presentation.exe`.
3. En el primer arranque se crea `%LocalAppData%\GeneradoNominaSystem\`
   con `data\app.db` (migraciones aplicadas solas) y `logs\`.

Para desinstalar: `scripts/desinstalar.ps1` (conserva los datos por defecto).

Sin registro de Windows en v1.0.0.

## Primer uso sugerido

1. Pestaña Empresas → crea tu empresa.
2. Empleados → registra 1–2 empleados.
3. Conceptos → Salario (devengo), Salud/Pensión (deducción, 4 %).
4. Periodos → el mes actual.
5. Nóminas → nueva, agrega detalles, calcula con base = salario, aprueba.
6. Exporta a PDF y revisa `Mis Documentos\GeneradoNominaSystem`.

## Backup y restauración

Con la aplicación **cerrada**:

- Backup: copia `%LocalAppData%\GeneradoNominaSystem\data\app.db` a un lugar seguro.
- Restore: devuelve el archivo a la misma ruta y abre la app.
- Los documentos exportados viven en `Mis Documentos\GeneradoNominaSystem`; respáldalos aparte.

## Solución de problemas

| Síntoma | Causa probable | Acción |
|---|---|---|
| Diálogo "Error crítico" al abrir (config) | `appsettings.json` dañado/faltante junto al exe | Re-copiar la carpeta publicada |
| Diálogo "Error crítico" de base de datos | `app.db` corrupta o bloqueada | Restaura el backup |
| Mensaje rojo en una vista | Validación de negocio (ej. NIT duplicado) | Lee el mensaje, corrige el dato |
| SmartScreen al ejecutar | Exe sin firma de código | "Más información → Ejecutar de todos modos" |
| App lenta con miles de registros | Listados sin paginación (deuda v1) | Filtra por fechas en Historial |

Logs: `%LocalAppData%\GeneradoNominaSystem\logs\log-*.txt` (rotación diaria, 30 días).
Los logs no contienen salarios ni documentos de identidad.

## Licencias de terceros (runtime)

| Paquete | Licencia |
|---|---|
| QuestPDF 2026.8.0 | Community (gratis si individuo o empresa < USD 1M/año; **excluye sector público y cotizadas**) — verificar elegibilidad antes de distribuir fuera de ese marco |
| ClosedXML, EF Core, MaterialDesign, Serilog, FluentValidation, Microsoft.Extensions | MIT / BSD-3 / Apache-2.0 |

Dependencias solo de tests (no se distribuyen): xUnit, Moq, FluentAssertions, coverlet.

## Deuda conocida (candidatas post-v1)

- Sin paginación en listados; N+1 al cargar detalles (bien hasta miles de registros).
- Personalización visual de documentos (logo/firma) usa defaults.
- Script de instalación con accesos directos (`scripts/instalar.ps1`), sin firma de código ni auto-actualización.
- Sin multiusuario/autenticación; `GeneradoPor` = "Sistema".
- Sin motor de fórmulas (porcentaje o valor fijo).
- Editar/quitar líneas solo donde existe (cotizaciones y plantillas sí; nóminas: anular+recrear).

## Verificación de este release

- `dotnet build` 0 warnings / 0 errors (Debug + Release).
- `dotnet test`: 37 + 68 + 22 + 5 + 6 = **138/138**.
- Publish `win-x64` self-contained verificado: runtime incluido, sin DLLs de tests ni de diseño.
- Smoke desde cero: BD migrada (13 tablas), `Iniciando v1.0.0`, cero errores, doble instancia OK, backup/restore con contenido OK.
