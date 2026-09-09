# Arquitectura — Generado Nomina System

> Fase 0 — Definición. Este documento es el contrato arquitectónico del proyecto.
> Si una decisión futura lo contradice, se actualiza este documento primero.

## 1. Objetivo

Aplicación de escritorio Windows (.exe) para gestión de nóminas personalizadas,
cotizaciones y generación de documentos PDF/Excel, con datos locales y
arquitectura extensible.

## 2. Estilo arquitectónico

**Clean Architecture simplificada** (no DDD completo, no CQRS, no Event Sourcing).

Capas:

```
Presentation (WPF + MVVM)
    |
    v
Application (casos de uso, DTOs, validación)
    |
    v
Domain (entidades, value objects, reglas, puertos/interfaces)
    ^
    |
Infrastructure (EF Core + SQLite, QuestPDF, ClosedXML, config, logging)
```

Regla fundamental: **las dependencias apuntan hacia Domain. Domain no depende de nada.**

```
Presentation -> Application -> Domain <- Infrastructure
```

### Por qué esta elección

- La lógica de nómina debe ser testeable sin UI ni base de datos.
- La generación de documentos debe ser intercambiable sin tocar el dominio.
- El proyecto debe crecer por módulos sin reescribir capas existentes.
- Se evita sobreingeniería: sin bounded contexts, sin aggregates complejos,
  sin bus de eventos en Fase 1.

## 3. Capas y responsabilidades

### 3.1 Domain (`GeneradoNominaSystem.Domain`)

Contiene:

- `Entities/`: Empresa, Empleado, Nomina, DetalleNomina, PeriodoNomina,
  ConceptoNomina, PlantillaNomina, PlantillaConcepto, Cotizacion,
  DetalleCotizacion, ProductoServicio, PlantillaCotizacion, Documento.
- `ValueObjects/`: Dinero, PeriodoFecha, DatosFiscales, Direccion,
  ConfiguracionDocumento.
- `Enums/`: TipoConcepto, EstadoNomina, TipoPeriodo, EstadoCotizacion,
  TipoDocumento, TipoContrato, EstadoEmpleado, FormatoExportacion, etc.
- `Exceptions/`: DominioException, ReglaNegocioException.
- `Interfaces/Repositories/`: contratos `IEmpresaRepository`, etc.
- `Interfaces/Services/`: contratos `IExportadorDocumento`, etc.
- `Services/`: ServicioCalculoNomina, ServicioNumeracion (lógica pura).

Prohibido en Domain:

- Referencias a `System.Data`, EF Core, WPF, QuestPDF, ClosedXML.
- Acceso a archivos, red, reloj del sistema directamente (inyectar abstracción si se necesita).
- DTOs de aplicación o ViewModels.

### 3.2 Application (`GeneradoNominaSystem.Application`)

Contiene:

- `DTOs/`: objetos planos para transporte entre UI y servicios.
- `Interfaces/`: `IEmpresaService`, `IEmpleadoService`, `INominaService`, etc.
- `Services/`: orquestación de casos de uso, llamadas a repositorios,
  aplicación de validaciones.
- `Validators/`: FluentValidation por DTO.

Reglas:

- No conoce WPF ni EF Core.
- No genera PDF/Excel directamente; usa `IExportadorDocumento`.
- Toda regla de cálculo vive en Domain; Application solo orquesta.

### 3.3 Infrastructure (`GeneradoNominaSystem.Infrastructure`)

Contiene:

- `Data/AppDbContext.cs` + `Data/Configurations/*` + `Data/Migrations/*`.
- `Repositories/*`: implementaciones EF Core de los puertos de Domain.
- `Exportadores/`: ExportadorPdf (QuestPDF), ExportadorExcel (ClosedXML),
  PlantillaRenderer.
- `Services/`: ConfiguracionService, ServicioNumeracionImpl.
- `Logging/`: configuración Serilog.

Reglas:

- Es la única capa que conoce SQLite, sistema de archivos y librerías externas.
- Si se cambia de PDF a otra librería, solo cambia `Exportadores/`.

### 3.4 Presentation (`GeneradoNominaSystem.Presentation`)

WPF + MVVM:

- `Views/`: XAML sin lógica de negocio.
- `ViewModels/`: estado, comandos, llamadas a Application.
- `Commands/RelayCommand.cs`: implementación única de ICommand.
- `Converters/`: conversión UI (ej. Estado -> color).
- `Resources/`: estilos MaterialDesign.

Reglas:

- ViewModel nunca accede a DbContext ni a repositorios directamente.
- Validación visual refleja errores de Application/Domain, no los reemplaza.

## 4. Dependency Injection

- Container: `Microsoft.Extensions.DependencyInjection`.
- Composition root: `App.xaml.cs` en Presentation.
- Domain y Application solo definen interfaces; Infrastructure y Application
  proveen implementaciones.
- Registro por módulo/extensión (ej. `AddInfrastructure()`, `AddApplication()`)
  a partir de Fase 2. No hacer scanning mágico de ensamblados.

## 5. Persistencia

- SQLite + EF Core 8.
- Un solo `AppDbContext`.
- `Fluent API` en `Configurations/` (no DataAnnotations en entidades de dominio).
- Migraciones EF Core versionadas en `Data/Migrations/`.
- Repository genérico `Repository<T>` + repositorios específicos solo cuando
  aporten queries propias.
- **No Unit of Work custom en Fase 1.** Se usa el `SaveChanges` del DbContext.
  Se introduce UoW solo si aparece una transacción multi-agregado real.

Ubicación BD local (definido en Fase 15, anticipado aquí):

- `%LocalAppData%/GeneradoNominaSystem/data/app.db`
- Nunca junto al .exe (evita problemas de permisos y pérdida en updates).

## 6. Generación de documentos (diseño extensible)

Flujo obligatorio:

```
Domain (Nomina/Cotizacion calculada)
  -> Application (DocumentoService prepara modelo inmutable)
    -> IExportadorDocumento.Exportar(modelo, ruta)
      -> PDF / Excel / futuro formato
```

Contrato (Fase 9):

```csharp
// Ilustrativo, no implementar en Fase 0
interface IExportadorDocumento
{
    FormatoExportacion Formato { get; }
    void Exportar(DocumentoModelo modelo, string rutaDestino);
}
```

- Agregar un formato nuevo = nueva clase que implementa la interfaz.
  Cero cambios en Domain/Application.
- Plantillas = JSON serializable (`ConfiguracionDocumento` + lista de conceptos).
  QuestPDF renderiza. Sin diseñador visual WYSIWYG en v1.

## 7. Configuración

- `appsettings.json` para configuración de app (rutas, logging, conexión).
- Configuración de negocio (moneda default COP, formatos, numeración,
  datos empresa) en base de datos, no en código.
- Moneda: value object `Dinero { Monto, Moneda }`. Default `COP`, multimoneda
  soportada desde el modelo. Formato numérico configurable por empresa.

## 8. Logging y errores

- Serilog con sink de archivo (rotación diaria). Consola solo en dev.
- Niveles: Information (casos de uso), Warning (validaciones), Error (excepciones).
- Excepciones de dominio: `DominioException` / `ReglaNegocioException`.
- Presentation muestra mensajes amigables; log guarda stack trace.
- Nunca loguear datos sensibles completos (documentos de identidad, salarios
  en modo verbose). Enmascarar si es necesario.

## 9. Validación

Dos niveles:

1. **Dominio**: invariantes (ej. monto no negativo, periodo fin > inicio).
2. **Application**: FluentValidation sobre DTOs (campos requeridos, formatos).

La UI refleja errores, no es la fuente de verdad.

## 10. Testing (estrategia)

- `Domain.Tests`: motor de cálculo, Dinero, reglas. Prioridad máxima.
- `Application.Tests`: servicios con repositorios mockeados.
- `Infrastructure.Tests`: repositorios con SQLite en memoria/file temporal,
  exportadores generan archivo y verifican existencia/tamaño básico.
- `IntegrationTests`: flujo Empleado -> Nomina -> Documento.
- Motor de nómina: tests deterministas (mismo input = mismo output), casos
  borde (cero, negativos, porcentajes, redondeos).

## 11. Estructura de solución (.NET)

```
src/
  GeneradoNominaSystem.Domain/
  GeneradoNominaSystem.Application/
  GeneradoNominaSystem.Infrastructure/
  GeneradoNominaSystem.Presentation/
tests/
  GeneradoNominaSystem.Domain.Tests/
  GeneradoNominaSystem.Application.Tests/
  GeneradoNominaSystem.Infrastructure.Tests/
  GeneradoNominaSystem.IntegrationTests/
```

Referencias:

- Application -> Domain
- Infrastructure -> Domain, Application
- Presentation -> Application, Infrastructure (solo el composition root `App.xaml.cs` para registrar DI; ViewModels solo usan Application)
- Tests -> proyecto correspondiente + mocks

## 12. Lo que esta arquitectura prohíbe explícitamente

- DbContext en Presentation o Application.
- Lógica de cálculo en ViewModels o Exportadores.
- Llamadas directas QuestPDF/ClosedXML desde Domain.
- Singleton estático para configuración o BD.
- Diseñador visual de plantillas en v1 (se evalúa post-release).
- Multi-idioma en v1 (solo español).
- Autenticación multiusuario en v1 (se deja puerta abierta, no se implementa).

## 13. Evolución prevista

- Nuevos formatos de exportación: implementar `IExportadorDocumento`.
- Nuevos tipos de concepto: extender `TipoConcepto` + motor, sin tocar UI base.
- Multi-empresa: ya modelado desde Fase 0 (`EmpresaId` en casi todo).
- Backups/restauración: módulo Infrastructure futuro, no toca dominio.
