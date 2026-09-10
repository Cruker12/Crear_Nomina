using FluentAssertions;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.ValueObjects;
using GeneradoNominaSystem.Infrastructure.Repositories;

namespace GeneradoNominaSystem.Infrastructure.Tests.Repositories;

public sealed class NominaRepositoryTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();

    [Fact]
    public async Task ObtenerConDetalles_NominaConDetalles_DeberiaIncluirlos()
    {
        using var context = _factory.Crear();
        var empresa = new Empresa(
            "Empresa Test S.A.S.",
            "Empresa Test",
            "900999111",
            new Direccion("Calle 1", "Bogotá", "Cundinamarca"),
            new DatosFiscales("900999111", "Empresa Test S.A.S."));
        var empleado = new Empleado(
            empresa.Id,
            TipoDocumentoIdentidad.Cedula,
            "12345678",
            "Juan",
            "Pérez",
            new DateTime(2024, 1, 15),
            "Auxiliar",
            new Dinero(1000000m, "COP"));
        var periodo = new PeriodoNomina(empresa.Id, "Marzo 2026", TipoPeriodo.Mensual, new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));
        var concepto = new ConceptoNomina("Salario", TipoConcepto.Devengo, 1, empresa.Id, valorFijo: new Dinero(1000000m, "COP"));

        await context.Set<Empresa>().AddAsync(empresa);
        await context.Set<Empleado>().AddAsync(empleado);
        await context.Set<PeriodoNomina>().AddAsync(periodo);
        await context.Set<ConceptoNomina>().AddAsync(concepto);
        await context.SaveChangesAsync();

        var nomina = new Nomina(empresa.Id, empleado.Id, periodo.Id);
        nomina.AgregarDetalle(new DetalleNomina(nomina.Id, concepto.Id, new Dinero(1000000m, "COP"), 1));
        await context.Set<Nomina>().AddAsync(nomina);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repo = new NominaRepository(context);
        var obtenida = await repo.ObtenerConDetallesAsync(nomina.Id);

        obtenida.Should().NotBeNull();
        obtenida!.Detalles.Should().HaveCount(1);
        obtenida.Detalles[0].Valor.Monto.Should().Be(1000000m);
    }

    public void Dispose() => _factory.Dispose();
}
