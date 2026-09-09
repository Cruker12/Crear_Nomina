# Roadmap — Generado Nomina System

> Fase 0. Plan incremental. No se avanza de fase sin cumplir criterios
> de aceptación. Cada fase entrega: objetivo, arquitectura, archivos,
> tareas, código, pruebas, verificación manual, riesgos y commit.

```
F0 -> F1 -> F2 -> F3 -> F4 -> F5 -> F6 -> F7 -> F8 -> F9
                                              F4 -> F10 -> F11
                                              F9 -> F12
                                              F6 -> F13
                                         F5-F12 -> F14 -> F15 -> F16 -> F17
```

## Fase 0 — Definición y decisiones técnicas (ESTA FASE)

- Objetivo: contrato arquitectónico, dominio, stack, roadmap, convenciones.
- Entrega: `docs/arquitectura.md`, `docs/dominio.md`, `docs/decisiones.md`,
  `docs/roadmap.md`, `README.md`.
- Sin código de producción.
- Criterios: decisiones del usuario documentadas, dominio cubre requisitos,
  roadmap ejecutable, estructura lista para Fase 1.

## Fase 1 — Inicialización del proyecto

- Objetivo: solución .NET 8 compilable + estructura + tests base.
- Crea: `GeneradoNominaSystem.sln`, 4 proyectos `src/*`, 4 proyectos `tests/*`,
  `.gitignore`, `.editorconfig`, estructura de carpetas vacía con `.gitkeep` si aplica.
- Paquetes base: xUnit, Moq, FluentAssertions en tests.
- Pruebas: `dotnet build`, `dotnet test` (1 test dummy por proyecto que pasa).
- Verificación manual: abrir .sln en Visual Studio, compilar en Debug/Release.
- NO: entidades, EF Core, WPF funcional, PDF/Excel.
- Commit: `chore: initialize solution structure with clean architecture layers`

## Fase 2 — Arquitectura base

- Objetivo: DI, logging, configuración, manejo de errores funcionando.
- Crea: `ServiceCollectionExtensions` por capa, `appsettings.json`,
  `ConfiguracionSerilog`, `DominioException`, `ReglaNegocioException`,
  `ViewModelBase`, `RelayCommand`.
- Pruebas: resolver servicios en tests, log escribe archivo, config lee valores.
- NO: base de datos, entidades de negocio.
- Commit: `feat: add application composition root with DI logging and configuration`

## Fase 3 — Dominio

- Objetivo: entidades, value objects, enums, interfaces, servicios puros.
- Crea: todo `Domain/Entities`, `ValueObjects` (Dinero primero),
  `Enums`, `Interfaces`, `ServicioNumeracion` (puro).
- `ServicioCalculoNomina` se define como contrato + casos simples; motor
  completo va en Fase 7.
- Pruebas: Dinero (suma, moneda distinta lanza), PeriodoFecha, Numeracion.
- NO: EF Core, UI, PDF.
- Commits: uno por agregado (ej. `feat: add empresa domain model`,
  `feat: add dinero value object`).

## Fase 4 — Persistencia

- Objetivo: SQLite + EF Core + repositorios + migración inicial.
- Crea: `AppDbContext`, `Configurations/*`, `Migrations/*`,
  `Repository<T>`, repositorios concretos.
- Pruebas: CRUD Empresa/Empleado en SQLite temporal, migración aplica y revierte.
- Verificación: abrir `app.db` con DB Browser, confirmar tablas.
- NO: UI, lógica de negocio.
- Commit: `feat: add sqlite persistence with ef core and repositories`

## Fase 5 — Empresas y configuración

- Objetivo: CRUD Empresa + configuración (moneda COP default, formatos, logo por ruta).
- Crea: DTO, Validator, Service, ViewModel, View.
- Pruebas: validator rechaza Nit vacío/duplicado, service crea/actualiza.
- Verificación manual: crear empresa, recargar app, persiste.
- NO: empleados, nómina.
- Commit: `feat: add empresa management with configuration`

## Fase 6 — Empleados

- Objetivo: CRUD Empleado vinculado a Empresa.
- Pruebas: unicidad (EmpresaId, NumeroDocumento), salario >= 0.
- Verificación: crear 3 empleados, filtrar por empresa, baja lógica.
- NO: cálculo de nómina.
- Commit: `feat: add empleado management with validations`

## Fase 7 — Motor de nómina

- Objetivo: Conceptos + cálculo determinista.
- Crea: ConceptoNomina CRUD, PeriodoNomina CRUD, ServicioCalculoNomina completo,
  Nomina + DetalleNomina.
- Pruebas (prioridad máxima): salario base, horas extra, porcentajes salud/pensión,
  deducciones, comisiones, redondeo, cero, valores negativos rechazados,
  orden de aplicación, determinismo (N ejecuciones mismo resultado).
- Verificación: tabla de casos manuales con calculadora.
- NO: plantillas, PDF.
- Commits: `feat: add payroll concept model`, `feat: add payroll calculation engine`,
  `test: add payroll calculation tests`.

## Fase 8 — Plantillas de nómina

- Objetivo: PlantillaNomina + PlantillaConcepto reutilizables.
- Pruebas: crear nómina desde plantilla precarga detalles.
- Verificación: aplicar plantilla a 2 empleados distintos.
- NO: PDF.
- Commit: `feat: add payroll templates with custom fields`

## Fase 9 — Generación de documentos

- Objetivo: `IExportadorDocumento` + PDF (QuestPDF) + Excel (ClosedXML).
- Pruebas: genera archivo, tamaño > 0, regeneración crea nuevo registro Documento.
- Verificación: abrir PDF/Excel generados, validar totales = pantalla.
- NO: cotizaciones aún.
- Commits: `feat: add document exporter abstraction`,
  `feat: add pdf payroll exporter`, `feat: add excel payroll exporter`.

## Fase 10 — Cotizaciones

- Objetivo: ProductoServicio + Cotizacion + DetalleCotizacion + cálculos.
- Pruebas: totales, descuentos %, vigencia >= emisión, número secuencial.
- Verificación: crear cotización con 5 líneas, validar total manual.
- NO: plantillas de cotización (Fase 11).
- Commit: `feat: add quotation management with totals calculation`

## Fase 11 — Plantillas de cotización

- Objetivo: PlantillaCotizacion + exportar cotización a PDF/Excel.
- Reúsa `IExportadorDocumento` (cero cambios en motor).
- Commit: `feat: add quotation templates and document export`

## Fase 12 — Históricos y gestión documental

- Objetivo: consulta de Nominas/Cotizaciones/Documentos con filtros y búsqueda.
- Pruebas: filtros por empresa, fechas, estado; Documento inmutable.
- Verificación: regenerar documento crea nuevo registro, el anterior intacto.
- Commit: `feat: add history and document management with filters`

## Fase 13 — UX/UI

- Objetivo: navegación, estados de carga, errores amigables, validación visual,
  temas MaterialDesign, atajos.
- Solo después de funcionalidad base. No rediseño completo.
- Commit: `feat: improve ux with validation feedback and navigation`

## Fase 14 — Testing completo

- Objetivo: cobertura de regresión, integration tests E2E
  (Empresa -> Empleados -> Nomina -> PDF), pruebas de persistencia y exportadores.
- Sin features nuevas. Solo tests + fixes que revelen.
- Commit: `test: add integration and regression coverage`

## Fase 15 — Build y .exe

- Objetivo: `dotnet publish` self-contained win-x64, `app.db` en LocalAppData,
  logs en LocalAppData, manejo de rutas, icono, metadatos.
- Verificación: .exe corre sin SDK instalado.
- Commit: `chore: configure production build and self-contained publish`

## Fase 16 — QA final

- Objetivo: pruebas en máquina limpia (sin dev tools), checklist de errores,
  backups, permisos, archivos faltantes.
- Sin features. Solo fixes.
- Commit(s): `fix: ...` por bug.

## Fase 17 — Release v1.0

- Objetivo: tag `v1.0.0`, notas de release, manual de instalación/uso,
  procedimiento de distribución y backup.
- Commit: `docs: add release v1.0 documentation and distribution guide`

## Reglas de avance

1. Una fase a la vez. No se inicia la siguiente sin criterios cumplidos.
2. Si se detecta sobreingeniería, deuda o violación SOLID, se señala y se
   corrige antes de avanzar.
3. Commits pequeños, atómicos, Conventional Commits.
4. Cada fase indica qué NO implementar.
