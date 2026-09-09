# Decisiones Técnicas — Generado Nomina System

> Fase 0. Decisiones confirmadas con el usuario. Cada decisión incluye
> alternativas evaluadas y motivo de elección.

## D1. Plataforma y lenguaje: C# / .NET 8 + WPF

Alternativas: WinUI 3, Python + PyQt, Java + JavaFX, Electron, Tauri, AvaloniaUI.

| Criterio | .NET+WPF | WinUI | PyQt | JavaFX | Electron/Tauri |
|---|:---:|:---:|:---:|:---:|:---:|
| .exe simple | 5 | 4 | 3 | 3 | 4 |
| Rendimiento | 5 | 5 | 3 | 4 | 3/5 |
| SQLite | 5 | 5 | 4 | 4 | 4 |
| PDF | 5 | 5 | 4 | 4 | 3 |
| Excel | 5 | 5 | 5 | 4 | 3 |
| UI nativa Win | 5 | 5 | 3 | 3 | 3 |
| Testing | 5 | 5 | 4 | 5 | 4 |
| Distribución | 5 | 4 | 3 | 2 | 4 |
| Futuros devs | 5 | 4 | 4 | 4 | 3 |
| Plantillas/doc | 5 | 4 | 3 | 3 | 2 |

**Elegido: .NET 8 (LTS) + WPF.**

Motivos: `dotnet publish` self-contained, arranque rápido, EF Core maduro,
MVVM nativo, ecosistema enorme, soporte Microsoft hasta nov-2026.
WinUI se descarta por menor madurez y distribución más compleja.
Python se descarta por empaquetado frágil y ejecutables pesados.
Java por JRE pesado. Electron/Tauri por memoria y UI no nativa.

## D2. Persistencia: SQLite + EF Core 8

Alternativas: SQLite + Dapper, JSON files, SQL Server LocalDB.

- **JSON**: descartado. Sin relaciones, sin integridad, sin queries.
- **Dapper**: más ligero pero sin migraciones; más SQL manual.
- **LocalDB**: requiere instalación SQL Server; rompe portabilidad del .exe.

**Elegido: SQLite + EF Core.** Migraciones versionadas, Fluent API,
un solo archivo `app.db`, cero instalación.

Puertos: `IRepository<T>` + repositorios específicos. Sin UoW custom en v1
(se usa `SaveChanges`; se introduce solo ante necesidad real).

## D3. PDF: QuestPDF (MIT)

Alternativas: iText7 (AGPL/comercial), PdfSharpCore (bajo nivel), IronPDF (comercial).

**Elegido: QuestPDF.** API declarativa en C#, layouts profesionales,
licencia MIT, sin dependencia de impresoras ni Office.

## D4. Excel: ClosedXML (MIT)

Alternativas: EPPlus (comercial desde v5), NPOI (API compleja).

**Elegido: ClosedXML.** Simple, sin Office instalado, suficiente para
nóminas/cotizaciones tabulares.

## D5. UI: WPF + MaterialDesignInXAML

Alternativas: WPF nativo, HandyControl, WinUI.

**Elegido: WPF + MaterialDesignInXAML.** UI profesional sin construir
design system propio. MVVM con `RelayCommand` propio (sin frameworks
MVVM pesados en v1 para evitar sobreingeniería).

## D6. Arquitectura: Clean Architecture simplificada

Alternativas: N-capas clásica, DDD completo, Vertical Slice, CQRS.

- N-capas clásica acopla UI a BD con DataSets; descartada.
- DDD completo (bounded contexts, aggregates estrictos, eventos) es
  overkill para este tamaño; se toman sus conceptos útiles (entidades, VO,
  servicios de dominio) sin el overhead.
- CQRS/Event Sourcing: innecesario en v1.

**Elegido: Clean Architecture de 4 capas** (ver `arquitectura.md`).

## D7. Validación: FluentValidation + invariantes de dominio

- Dominio valida invariantes (montos, fechas, estados).
- Application valida DTOs con FluentValidation.
- UI solo refleja errores.

## D8. Logging: Serilog (archivo, rotación diaria)

Alternativas: NLog, log built-in. Serilog por sinks maduros y configuración simple.

Prohibido loguear documentos de identidad o salarios en verbose sin enmascarar.

## D9. DI: Microsoft.Extensions.DependencyInjection

Sin contenedores de terceros (Autofac, etc.) en v1. Suficiente y estándar.

## D10. Configuración

- `appsettings.json` para app (rutas, logging, connection string).
- Negocio (moneda, formatos, numeración) en BD por empresa.

## D11. Moneda

- VO `Dinero { Monto, Moneda }`.
- Default **COP**, multimoneda soportada desde el modelo.
- Formato (símbolo, decimales) configurable por empresa/documento.
- Operaciones solo misma moneda en v1 (sin conversión automática; se evita
  integrar tasas de cambio por sobreingeniería).

## D12. Idioma

Solo español en v1. Sin i18n para no multiplicar costo de UI/documentos.

## D13. Plantillas de documentos

Alternativas: diseñador visual WYSIWYG, XAML DataTemplates editables,
JSON + renderer.

**Elegido: JSON (`ConfiguracionDocumento`) + QuestPDF/ClosedXML renderer.**
Serializable, versionable, suficiente para logo, encabezado/pie,
firmas, numeración, orden de campos, formatos.

Diseñador visual: explícitamente fuera de v1 (complejidad enorme).

## D14. Logo de empresa

Se guarda **ruta de archivo**, no `byte[]` en SQLite. Evita inflar la BD
y simplifica backups.

## D15. Cliente de cotización

En v1 es **datos embebidos en Cotizacion**, no entidad Cliente separada.
Si a futuro se necesita CRM/cartera, se extrae sin romper cotizaciones
(precio/descripción ya quedan copiados como histórico inmutable).

## D16. Usuarios/autenticación

Fuera de v1. `Documento.GeneradoPor` es string libre. Arquitectura deja
puerta abierta (columna preparada, sin módulo de auth).

## D17. Testing

xUnit + Moq + FluentAssertions. SQLite en memoria/archivo temporal para
tests de repositorio. Prioridad: motor de cálculo determinista.

## D18. Distribución

`dotnet publish` self-contained win-x64. BD en `%LocalAppData%`, nunca junto
al exe. Instalador (Inno Setup) se decide en Fase 15, no antes.

## D19. Decisiones explícitamente diferidas (no hacer ahora)

- Motor de fórmulas/expresiones para conceptos (v1: porcentaje o valor fijo).
- Diseñador visual de plantillas.
- Multi-idioma.
- Multi-usuario/auth.
- Sincronización nube.
- Tasas de cambio automáticas.
