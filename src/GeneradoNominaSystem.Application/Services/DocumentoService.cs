using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Comun;
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
    private readonly INotificadorDocumentos _notificador;

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
        IEnumerable<IExportadorDocumento> exportadores,
        INotificadorDocumentos notificador)
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
        _notificador = notificador;
    }

    public async Task<DocumentoDto> ExportarNominaAsync(
        Guid nominaId,
        FormatoExportacion formato,
        string rutaDestino,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(rutaDestino))
        {
            throw new ReglaNegocioException("La ruta de destino es obligatoria.");
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
        var ruta = ResolverRutaUnica(rutaDestino, extension);

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
        _notificador.Notificar(documento.ReferenciaId, ruta);

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
        string rutaDestino,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(rutaDestino))
        {
            throw new ReglaNegocioException("La ruta de destino es obligatoria.");
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
        var ruta = ResolverRutaUnica(rutaDestino, extension);

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
        _notificador.Notificar(documento.ReferenciaId, ruta);

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

    /// <summary>
    /// Respeta el nombre elegido por el usuario; si el archivo ya existe,
    /// agrega sufijo _2, _3, ... Crea el directorio si no existe.
    /// </summary>
    private static string ResolverRutaUnica(string rutaDeseada, string extension)
    {
        var directorioDeseado = Path.GetDirectoryName(rutaDeseada);
        var nombreSaneado = NombresArchivos.Sanear(Path.GetFileName(rutaDeseada));
        var combinada = string.IsNullOrWhiteSpace(directorioDeseado)
            ? nombreSaneado
            : Path.Combine(directorioDeseado, nombreSaneado);
        var ruta = Path.GetFullPath(Path.ChangeExtension(combinada, extension));
        var directorio = Path.GetDirectoryName(ruta);
        if (!string.IsNullOrWhiteSpace(directorio))
        {
            Directory.CreateDirectory(directorio);
        }

        if (!File.Exists(ruta))
        {
            return ruta;
        }

        var baseNombre = Path.GetFileNameWithoutExtension(ruta);
        var contador = 2;
        string candidata;
        do
        {
            candidata = Path.Combine(directorio!, $"{baseNombre}_{contador}.{extension}");
            contador++;
        }
        while (File.Exists(candidata));

        return candidata;
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
