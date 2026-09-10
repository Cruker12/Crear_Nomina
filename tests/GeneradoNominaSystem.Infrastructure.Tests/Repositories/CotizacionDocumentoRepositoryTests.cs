using FluentAssertions;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.ValueObjects;
using GeneradoNominaSystem.Infrastructure.Repositories;

namespace GeneradoNominaSystem.Infrastructure.Tests.Repositories;

public sealed class CotizacionDocumentoRepositoryTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();

    private static Empresa CrearEmpresa(string nit)
    {
        return new Empresa(
            "Empresa Test S.A.S.",
            "Empresa Test",
            nit,
            new Direccion("Calle 1", "Bogotá", "Cundinamarca"),
            new DatosFiscales(nit, "Empresa Test S.A.S."));
    }

    [Fact]
    public async Task ObtenerPorNumero_Existente_DeberiaRetornarla()
    {
        using var context = _factory.Crear();
        var empresa = CrearEmpresa("900444111");
        var cotizacion = new Cotizacion(
            empresa.Id, "Cliente", "COT-2026-0007",
            new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));
        await context.Set<Empresa>().AddAsync(empresa);
        await context.Set<Cotizacion>().AddAsync(cotizacion);
        await context.SaveChangesAsync();

        var repo = new CotizacionRepository(context);
        var obtenida = await repo.ObtenerPorNumeroAsync(empresa.Id, "COT-2026-0007");

        obtenida.Should().NotBeNull();
        obtenida!.ClienteNombre.Should().Be("Cliente");
    }

    [Fact]
    public async Task ListarPorReferencia_ConDosDocumentos_DeberiaRetornarlos()
    {
        using var context = _factory.Crear();
        var empresa = CrearEmpresa("900444222");
        var referenciaId = Guid.NewGuid();
        await context.Set<Empresa>().AddAsync(empresa);
        await context.Set<Documento>().AddRangeAsync(
            new Documento(empresa.Id, TipoDocumentoSistema.Nomina, referenciaId, "NOM-2026-0001", FormatoExportacion.Pdf, "a.pdf", 100, "Sistema"),
            new Documento(empresa.Id, TipoDocumentoSistema.Nomina, referenciaId, "NOM-2026-0001", FormatoExportacion.Excel, "a.xlsx", 200, "Sistema"));
        await context.SaveChangesAsync();

        var repo = new DocumentoRepository(context);
        var lista = await repo.ListarPorReferenciaAsync(referenciaId);

        lista.Should().HaveCount(2);
    }

    public void Dispose() => _factory.Dispose();
}
