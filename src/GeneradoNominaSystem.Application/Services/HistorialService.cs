using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Enums;

namespace GeneradoNominaSystem.Application.Services;

public sealed class HistorialService : IHistorialService
{
    private readonly INominaService _nominas;
    private readonly ICotizacionService _cotizaciones;
    private readonly IDocumentoService _documentos;

    public HistorialService(
        INominaService nominas,
        ICotizacionService cotizaciones,
        IDocumentoService documentos)
    {
        _nominas = nominas;
        _cotizaciones = cotizaciones;
        _documentos = documentos;
    }

    public async Task<IReadOnlyList<NominaDto>> BuscarNominasAsync(
        Guid empresaId,
        string? texto = null,
        EstadoNomina? estado = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken ct = default)
    {
        var lista = (await _nominas.ListarPorEmpresaAsync(empresaId, ct)).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            lista = lista.Where(n =>
                n.EmpleadoNombre.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                n.PeriodoNombre.Contains(t, StringComparison.OrdinalIgnoreCase));
        }

        if (estado.HasValue)
        {
            lista = lista.Where(n => n.Estado == estado.Value);
        }

        if (desde.HasValue)
        {
            lista = lista.Where(n => n.FechaCreacion.Date >= desde.Value.Date);
        }

        if (hasta.HasValue)
        {
            lista = lista.Where(n => n.FechaCreacion.Date <= hasta.Value.Date);
        }

        return lista.OrderByDescending(n => n.FechaCreacion).ToList();
    }

    public async Task<IReadOnlyList<CotizacionDto>> BuscarCotizacionesAsync(
        Guid empresaId,
        string? texto = null,
        EstadoCotizacion? estado = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken ct = default)
    {
        var lista = (await _cotizaciones.ListarPorEmpresaAsync(empresaId, ct)).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            lista = lista.Where(c =>
                c.ClienteNombre.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                c.NumeroCotizacion.Contains(t, StringComparison.OrdinalIgnoreCase));
        }

        if (estado.HasValue)
        {
            lista = lista.Where(c => c.Estado == estado.Value);
        }

        if (desde.HasValue)
        {
            lista = lista.Where(c => c.FechaEmision.Date >= desde.Value.Date);
        }

        if (hasta.HasValue)
        {
            lista = lista.Where(c => c.FechaEmision.Date <= hasta.Value.Date);
        }

        return lista.OrderByDescending(c => c.FechaEmision).ToList();
    }

    public async Task<IReadOnlyList<DocumentoDto>> ListarDocumentosAsync(
        Guid empresaId,
        TipoDocumentoSistema? tipo = null,
        FormatoExportacion? formato = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken ct = default)
    {
        var query = (await _documentos.ListarPorEmpresaAsync(empresaId, ct)).AsEnumerable();
        if (tipo.HasValue)
        {
            query = query.Where(d => d.Tipo == tipo.Value);
        }

        if (formato.HasValue)
        {
            query = query.Where(d => d.Formato == formato.Value);
        }

        if (desde.HasValue)
        {
            query = query.Where(d => d.FechaGeneracion.Date >= desde.Value.Date);
        }

        if (hasta.HasValue)
        {
            query = query.Where(d => d.FechaGeneracion.Date <= hasta.Value.Date);
        }

        return query.OrderByDescending(d => d.FechaGeneracion).ToList();
    }
}
