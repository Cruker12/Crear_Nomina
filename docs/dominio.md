# Dominio — Generado Nomina System

> Fase 0. Modelo conceptual. Los nombres y formas son propuesta definitiva
> para v1 salvo que una fase posterior justifique un cambio (se documenta).

## 1. Resumen del dominio

El sistema gestiona dos flujos principales que comparten Empresa,
configuración y generación de documentos:

1. **Nómina**: Empresa -> Empleados -> Periodo -> Conceptos -> Cálculo -> Documento.
2. **Cotización**: Empresa -> Productos/Servicios -> Cotización -> Documento.

Todo documento generado queda registrado en Históricos (`Documento`).

## 2. Entidades

### 2.1 Empresa

Cliente del sistema. Todo cuelga de Empresa (multi-empresa desde v1 a nivel modelo).

Campos:

- Id (Guid)
- RazonSocial (string, requerido)
- NombreComercial (string, requerido)
- Nit (string, requerido, único)
- Direccion (Direccion VO)
- Telefono, Email
- Logo (ruta archivo, no binario en BD — decisión: evita inflar SQLite)
- DatosFiscales (DatosFiscales VO)
- Activo (bool)
- FechaCreacion, FechaModificacion?

Reglas:

- Nit único en el sistema.
- No se elimina físicamente si tiene Empleados/Nominas/Cotizaciones (baja lógica).

### 2.2 Empleado

Persona vinculada a una Empresa.

Campos:

- Id, EmpresaId (FK)
- TipoDocumento (enum), NumeroDocumento
- Nombres, Apellidos
- Email?, Telefono?, Direccion (VO)
- FechaIngreso
- Cargo, Departamento?
- TipoContrato (enum)
- SalarioBase (Dinero VO)
- Estado (Activo, Inactivo, Licencia)
- FechaCreacion, FechaModificacion?

Reglas:

- (EmpresaId, NumeroDocumento) único.
- SalarioBase >= 0.
- FechaIngreso no futura.
- No eliminar si tiene Nóminas (baja lógica).

### 2.3 PeriodoNomina

Ventana de liquidación.

- Id, EmpresaId
- Nombre ("Quincena Ene 2026", "Marzo 2026")
- Tipo (Diario, Semanal, Quincenal, Mensual, Especial)
- FechaInicio, FechaFin
- Anio, Mes?
- Activo

Reglas:

- FechaFin > FechaInicio.
- No solapamiento de periodos activos del mismo tipo y empresa
  (validación en Application/Domain service).

### 2.4 ConceptoNomina

Pieza configurable del cálculo. Es el corazón de la personalización.

- Id, EmpresaId (null = global del sistema)
- Nombre ("Salario Base", "Horas Extra", "Salud", "Pensión")
- Tipo (Devengo, Deduccion, Beneficio, Comision)
- Subtipo? ("Salud", "Pension", "Cesantias"...)
- EsPorcentaje (bool), PorcentajeBase? (decimal)
- ValorFijo? (Dinero)
- FormulaCalculo? (string — referencia simple, no motor de scripting en v1)
- Orden (int)
- RequiereBase (bool), AfectaBase (bool)
- Activo

Reglas:

- Si EsPorcentaje, PorcentajeBase entre 0 y 100.
- Si no es porcentaje, debe tener ValorFijo o RequiereBase = true
  (el valor llega al calcular la nómina).
- Orden define secuencia de aplicación en el motor.

Tipos:

- **Devengo**: suma al empleado (salario, horas, bonificación).
- **Deduccion**: resta (salud, pensión, préstamos).
- **Beneficio**: no monetario o informativo según plantilla (se parametriza).
- **Comision**: devengo variable por ventas/logros.

### 2.5 Nomina

Cabecera del cálculo para un Empleado en un Periodo.

- Id, EmpresaId, EmpleadoId, PeriodoNominaId, PlantillaNominaId?
- Estado (Borrador, Calculada, Aprobada, Pagada, Anulada)
- FechaCalculo?, FechaAprobacion?
- Observaciones?
- SubtotalDevengos, SubtotalDeducciones, TotalNeto (Dinero)
- FechaCreacion, FechaModificacion?

Reglas (máquina de estados):

```
Borrador -> Calculada -> Aprobada -> Pagada
   |            |            |
   +-----> Anulada <---------+
```

- Solo Borrador permite editar detalles.
- Solo Calculada puede aprobarse.
- Anulada es terminal (requiere motivo en Observaciones).
- TotalNeto = SubtotalDevengos - SubtotalDeducciones. Calculado por
  ServicioCalculoNomina, nunca a mano en UI.

### 2.6 DetalleNomina

Línea de la nómina.

- Id, NominaId, ConceptoNominaId
- Valor (Dinero)
- Cantidad? (horas, días)
- Descripcion?
- Orden

Reglas:

- Valor >= 0 (el signo lo da TipoConcepto, no el valor).
- Un concepto no se repite en la misma nómina salvo que la plantilla lo permita
  (por defecto: no repetir).

### 2.7 PlantillaNomina + PlantillaConcepto

Configuración reutilizable.

- PlantillaNomina: Id, EmpresaId, Nombre, Descripcion?, ConfiguracionDocumento (VO), Activo.
- PlantillaConcepto: PlantillaNominaId, ConceptoNominaId, Orden, Obligatorio, ValorPorDefecto?

Uso: al crear una Nómina desde plantilla, se precargan los detalles.

### 2.8 Cotizacion

- Id, EmpresaId
- ClienteNombre, ClienteDocumento?, ClienteEmail?, ClienteTelefono?, ClienteDireccion (VO?)
- NumeroCotizacion (string secuencial, único por empresa)
- FechaEmision, FechaVigencia
- Estado (Borrador, Enviada, Aceptada, Rechazada, Vencida)
- Observaciones?
- Subtotal, TotalDescuentos, TotalImpuestos?, TotalNeto (Dinero)
- Moneda (string, default COP)
- PlantillaCotizacionId?
- FechaCreacion, FechaModificacion?

Reglas:

- FechaVigencia >= FechaEmision.
- TotalNeto = Subtotal - Descuentos + Impuestos.
- NumeroCotizacion generado por ServicioNumeracion, no editable.

### 2.9 DetalleCotizacion

- Id, CotizacionId, ProductoServicioId?
- Descripcion (requerido, copia del producto para histórico inmutable)
- Cantidad (> 0), PrecioUnitario (Dinero >= 0)
- DescuentoPorcentaje? (0-100), DescuentoValor?
- Subtotal (Dinero calculado)
- Orden

### 2.10 ProductoServicio

Catálogo de la empresa.

- Id, EmpresaId, Nombre, Descripcion?, Unidad, PrecioUnitario (Dinero), Activo.

### 2.11 PlantillaCotizacion

- Id, EmpresaId, Nombre, ConfiguracionDocumento (VO), Activo.

### 2.12 Documento (histórico)

Registro de cada archivo generado.

- Id, EmpresaId
- Tipo (Nomina, Cotizacion)
- ReferenciaId (Guid de la Nómina/Cotización)
- NumeroDocumento, Formato (PDF, Excel)
- RutaArchivo, TamanoBytes
- FechaGeneracion, GeneradoPor?

Regla: inmutable. Si se regenera, se crea un nuevo registro.

## 3. Value Objects

### Dinero

```csharp
// Ilustrativo
Dinero { Monto: decimal, Moneda: string }
```

- Monto con 2 decimales (redondeo bancario controlado en motor).
- Moneda ISO ("COP", "USD", "EUR"). Default COP configurable por empresa.
- Operaciones solo entre misma moneda; si difieren -> excepción de dominio.
- Formateo según ConfiguracionDocumento (símbolo, decimales).

### PeriodoFecha

- FechaInicio, FechaFin. Valida Fin > Inicio.

### DatosFiscales

- Nit, DigitoVerificacion?, RazonSocial, RegimenTributario?, ResponsableIva, RepresentanteLegal?

### Direccion

- DireccionCompleta, Ciudad, Departamento, Pais (default "Colombia"), CodigoPostal?

### ConfiguracionDocumento

Guarda personalización de plantillas (serializable a JSON):

- MostrarLogo, LogoPosicion (Izquierda, Derecha, Centro)
- EncabezadoPersonalizado?, PiePaginaPersonalizado?
- MostrarFirma, TextoFirma?
- NumeracionAutomatica, PrefijoNumeracion?, SiguienteNumero
- FormatoFecha (default "dd/MM/yyyy")
- SimboloMoneda (default "$"), DecimalesMoneda (default 2)

## 4. Enums

- TipoConcepto: Devengo, Deduccion, Beneficio, Comision
- EstadoNomina: Borrador, Calculada, Aprobada, Pagada, Anulada
- TipoPeriodo: Diario, Semanal, Quincenal, Mensual, Especial
- EstadoCotizacion: Borrador, Enviada, Aceptada, Rechazada, Vencida
- TipoDocumentoIdentidad: Cedula, TarjetaIdentidad, CedulaExtranjeria, Pasaporte, NIT
- TipoContrato: Indefinido, Fijo, ObraLabor, Aprendizaje
- EstadoEmpleado: Activo, Inactivo, Licencia
- FormatoExportacion: PDF, Excel
- TipoDocumentoSistema: Nomina, Cotizacion
- PosicionLogo: Izquierda, Derecha, Centro

## 5. Servicios de dominio

### ServicioCalculoNomina

Entrada: empleado + lista de (concepto, valor/cantidad).
Salida: SubtotalDevengos, SubtotalDeducciones, TotalNeto + desglose.

Propiedades exigidas:

- Determinista.
- Orden-respetuoso (aplica conceptos por `Orden`).
- Sin I/O, sin BD, sin fecha actual implícita.
- Testeado exhaustivamente (Fase 7).

### ServicioNumeracion

`GenerarNumero(prefijo, anio, secuencia)` -> `"COT-2026-0045"`, `"NOM-2026-0001"`.
Secuencia por empresa + tipo. Persistida en configuración empresa.

## 6. Relaciones (resumen)

```
Empresa 1--* Empleado
Empresa 1--* PeriodoNomina
Empresa 1--* ConceptoNomina
Empresa 1--* PlantillaNomina
Empresa 1--* Cotizacion
Empresa 1--* ProductoServicio
Empresa 1--* Documento

Empleado 1--* Nomina
PeriodoNomina 1--* Nomina
PlantillaNomina 1--* PlantillaConcepto *--1 ConceptoNomina
Nomina 1--* DetalleNomina *--1 ConceptoNomina

Cotizacion 1--* DetalleCotizacion
ProductoServicio 1--* DetalleCotizacion (opcional, copia descriptiva)
```

## 7. Decisiones de modelado pendientes (no bloquean Fase 1-4)

- `Beneficio`: si resta/suma o es informativo se define en Fase 7 con casos reales.
- `FormulaCalculo`: en v1 es referencia simple, no lenguaje de fórmulas.
  Motor de expresiones se evalúa post-v1 solo si se necesita.
- Cliente de cotización: en v1 es datos embebidos en Cotizacion, no entidad
  Cliente separada (evita sobreingeniería; se extrae si se necesita CRM).
- Usuario/autenticación: fuera de v1. `GeneradoPor` es string libre.
