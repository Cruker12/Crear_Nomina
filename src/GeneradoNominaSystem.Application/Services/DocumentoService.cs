using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Documentos;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Domain.Interfaces.Services;

namespace GeneradoNominaSystem.Application.Services;

public sealed class DocumentoService : IDocumentoService
{
    private readonly INominaRepository _nominas;
    private readonly ICotizacionRepository _cotizaciones;
    private readonly IProductoServicioRepository _productosServicios;
    private readonly IEmpresaRepository _empresas;
    private readonly IEmpleadoRepository _empleados;
    private readonly IPeriodoNominaRepository _periodos;
    private readonly IConceptoNominaRepository _conceptos;
    private readonly IDocumentoRepository _documentos;
    private readonly IUnitOfWork _uow;
    private readonly IServicioNumeracion _numeracion;
    private readonly IEnumerable<IExportadorDocumento> _exportadores;

    public DocumentoService(
        INominaRepository nominas,
        ICotizacionRepository cotizaciones,
        IProductoServicioRepository productosServicios,
        IEmpresaRepository empresas,
        IEmpleadoRepository empleados,
        IPeriodoNominaRepository periodos,
        IConceptoNominaRepository conceptos,
        IDocumentoRepository documentos,
        IUnitOfWork uow,
        IServicioNumeracion numeracion,
        IEnumerable<IExportadorDocumento> exportadores)
    {
        _nominas = nominas;
        _cotizaciones = cotizaciones;
        _productosServicios = productosServicios;
        _empresas = empresas;
        _empleados = empleados;
        _periodos = periodos;
        _conceptos = conceptos;
        _documentos = documentos;
        _uow = uow;
        _numeracion = numeracion;
        _exportadores = exportadores;
    }

    public async Task<DocumentoDto> ExportarNominaAsync(
        Guid nominaId,
        FormatoExportacion formato,
        string carpetaDestino,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(carpetaDestino))
        {
            throw new ReglaNegocioException("La carpeta de destino es obligatoria.");
        }

        var nomina = await _nominas.ObtenerConDetallesAsync(nominaId, ct);
        if (nomina is null)
        {
            throw new ReglaNegocioException("La nómina no existe.");
        }

        if (nomina.Estado == EstadoNomina.Borrador)
        {
            throw new ReglaNegocioException("Solo se pueden exportar nóminas calculadas, aprobadas o pagadas.");
        }

        var exportador = _exportadores.FirstOrDefault(e => e.Formato == formato);
        if (exportador is null)
        {
            throw new ReglaNegocioException($"No hay exportador registrado para el formato {formato}.");
        }

        var modelo = await ConstruirModeloAsync(nomina, ct);
        var extension = formato == FormatoExportacion.Pdf ? "pdf" : "xlsx";
        var ruta = ResolverRutaUnica(carpetaDestino, Sanear(modelo.NumeroDocumento), extension);

        exportador.ExportarNomina(modelo, ruta);

        var tamano = new FileInfo(ruta).Length;
        var documento = new Documento(
            nomina.EmpresaId,
            TipoDocumentoSistema.Nomina,
            nomina.Id,
            modelo.NumeroDocumento,
            formato,
            ruta,
            tamano,
            "Sistema");

        await _documentos.AgregarAsync(documento, ct);
        await _uow.GuardarCambiosAsync(ct);

        return Mapear(documento);
    }

    public async Task<IReadOnlyList<DocumentoDto>> ListarPorReferenciaAsync(Guid referenciaId, CancellationToken ct = default)
    {
        var entidades = await _documentos.ListarPorReferenciaAsync(referenciaId, ct);
        return entidades.Select(Mapear).ToList();
    }

    public async Task<IReadOnlyList<DocumentoDto>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        var entidades = await _documentos.ListarPorEmpresaAsync(empresaId, ct);
        return entidades.Select(Mapear).ToList();
    }

    public async Task<DocumentoDto> ExportarCotizacionAsync(
        Guid cotizacionId,
        FormatoExportacion formato,
        string carpetaDestino,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(carpetaDestino))
        {
            throw new ReglaNegocioException("La carpeta de destino es obligatoria.");
        }

        var cotizacion = await _cotizaciones.ObtenerConDetallesAsync(cotizacionId, ct);
        if (cotizacion is null)
        {
            throw new ReglaNegocioException("La cotización no existe.");
        }

        if (cotizacion.Detalles.Count == 0)
        {
            throw new ReglaNegocioException("La cotización no tiene detalles para exportar.");
        }

        var exportador = _exportadores.FirstOrDefault(e => e.Formato == formato);
        if (exportador is null)
        {
            throw new ReglaNegocioException($"No hay exportador registrado para el formato {formato}.");
        }

        var modelo = await ConstruirModeloCotizacionAsync(cotizacion, ct);
        var extension = formato == FormatoExportacion.Pdf ? "pdf" : "xlsx";
        var ruta = ResolverRutaUnica(carpetaDestino, Sanear(modelo.NumeroCotizacion), extension);

        exportador.ExportarCotizacion(modelo, ruta);

        var tamano = new FileInfo(ruta).Length;
        var documento = new Documento(
            cotizacion.EmpresaId,
            TipoDocumentoSistema.Cotizacion,
            cotizacion.Id,
            modelo.NumeroCotizacion,
            formato,
            ruta,
            tamano,
            "Sistema");

        await _documentos.AgregarAsync(documento, ct);
        await _uow.GuardarCambiosAsync(ct);

        return Mapear(documento);
    }

    private async Task<ModeloDocumentoNomina> ConstruirModeloAsync(Nomina nomina, CancellationToken ct)
    {
        var empresa = await _empresas.ObtenerPorIdAsync(nomina.EmpresaId, ct);
        var empleado = await _empleados.ObtenerPorIdAsync(nomina.EmpleadoId, ct);
        var periodo = await _periodos.ObtenerPorIdAsync(nomina.PeriodoNominaId, ct);
        var conceptos = await _conceptos.ListarActivosAsync(nomina.EmpresaId, ct);
        var porId = conceptos.ToDictionary(c => c.Id);

        var existentes = await _documentos.ListarPorEmpresaAsync(nomina.EmpresaId, ct);
        var secuencia = existentes.Count(d => d.Tipo == TipoDocumentoSistema.Nomina) + 1;
        var numero = _numeracion.GenerarNumero("NOM", DateTime.UtcNow.Year, secuencia);

        var lineas = nomina.Detalles
            .OrderBy(d => d.Orden)
            .Select(d => new LineaDocumentoNomina(
                porId.TryGetValue(d.ConceptoNominaId, out var c) ? c.Nombre : d.ConceptoNominaId.ToString(),
                porId.TryGetValue(d.ConceptoNominaId, out var c2) ? c2.Tipo : TipoConcepto.Devengo,
                d.Cantidad,
                d.Valor.Monto,
                d.Valor.Moneda,
                d.Orden))
            .ToList();

        return new ModeloDocumentoNomina(
            empresa?.RazonSocial ?? "Empresa",
            empresa?.Nit ?? string.Empty,
            empresa?.Direccion.DireccionCompleta,
            empresa?.Telefono,
            empleado is null ? string.Empty : $"{empleado.Nombres} {empleado.Apellidos}",
            empleado?.NumeroDocumento ?? string.Empty,
            periodo?.Nombre ?? string.Empty,
            numero,
            DateTime.UtcNow,
            nomina.Estado.ToString(),
            lineas,
            nomina.SubtotalDevengos.Monto,
            nomina.SubtotalDeducciones.Monto,
            nomina.TotalNeto.Monto,
            nomina.TotalNeto.Moneda);
    }

    private async Task<ModeloDocumentoCotizacion> ConstruirModeloCotizacionAsync(Cotizacion cotizacion, CancellationToken ct)
    {
        var empresa = await _empresas.ObtenerPorIdAsync(cotizacion.EmpresaId, ct);

        var lineas = cotizacion.Detalles
            .OrderBy(d => d.Orden)
            .Select(d => new LineaDocumentoCotizacion(
                d.Descripcion,
                d.Cantidad,
                d.PrecioUnitario.Monto,
                d.DescuentoPorcentaje,
                d.Subtotal.Monto,
                d.Orden))
            .ToList();

        return new ModeloDocumentoCotizacion(
            empresa?.RazonSocial ?? "Empresa",
            empresa?.Nit ?? string.Empty,
            empresa?.Direccion.DireccionCompleta,
            empresa?.Telefono,
            cotizacion.ClienteNombre,
            cotizacion.ClienteDocumento,
            cotizacion.NumeroCotizacion,
            cotizacion.FechaEmision,
            cotizacion.FechaVigencia,
            cotizacion.Estado.ToString(),
            lineas,
            cotizacion.Subtotal.Monto,
            cotizacion.TotalNeto.Monto,
            cotizacion.Moneda,
            cotizacion.Observaciones);
    }

    private static string Sanear(string nombre)
    {
        foreach (var invalido in Path.GetInvalidFileNameChars())
        {
            nombre = nombre.Replace(invalido, '_');
        }

        return nombre;
    }

    private static string ResolverRutaUnica(string carpeta, string baseNombre, string extension)
    {
        Directory.CreateDirectory(carpeta);
        var sello = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var ruta = Path.Combine(carpeta, $"{baseNombre}_{sello}.{extension}");
        var contador = 2;
        while (File.Exists(ruta))
        {
            ruta = Path.Combine(carpeta, $"{baseNombre}_{sello}_{contador}.{extension}");
            contador++;
        }

        return ruta;
    }

    private static DocumentoDto Mapear(Documento d)
    {
        return new DocumentoDto
        {
            Id = d.Id,
            Tipo = d.Tipo,
            ReferenciaId = d.ReferenciaId,
            NumeroDocumento = d.NumeroDocumento,
            Formato = d.Formato,
            RutaArchivo = d.RutaArchivo,
            TamanoBytes = d.TamanoBytes,
            FechaGeneracion = d.FechaGeneracion,
        };
    }
}
