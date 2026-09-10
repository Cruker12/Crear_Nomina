using FluentAssertions;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.ValueObjects;
using GeneradoNominaSystem.Infrastructure.Repositories;

namespace GeneradoNominaSystem.Infrastructure.Tests.Repositories;

public sealed class PlantillaNominaRepositoryTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();

    [Fact]
    public async Task ObtenerConConceptos_PlantillaConConceptos_DeberiaIncluirlos()
    {
        using var context = _factory.Crear();
        var empresa = new Empresa(
            "Empresa Test S.A.S.",
            "Empresa Test",
            "900888777",
            new Direccion("Calle 1", "Bogotá", "Cundinamarca"),
            new DatosFiscales("900888777", "Empresa Test S.A.S."));
        var empresaId = empresa.Id;
        var concepto = new ConceptoNomina("Salario", TipoConcepto.Devengo, 1, empresaId, valorFijo: new Dinero(1000000m, "COP"));
        await context.Set<Empresa>().AddAsync(empresa);
        await context.Set<ConceptoNomina>().AddAsync(concepto);
        await context.SaveChangesAsync();

        var plantilla = new PlantillaNomina(empresaId, "Base");
        plantilla.AgregarConcepto(new PlantillaConcepto(plantilla.Id, concepto.Id, 1, true, new Dinero(1000000m, "COP")));
        await context.Set<PlantillaNomina>().AddAsync(plantilla);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repo = new PlantillaNominaRepository(context);
        var obtenida = await repo.ObtenerConConceptosAsync(plantilla.Id);

        obtenida.Should().NotBeNull();
        obtenida!.Conceptos.Should().HaveCount(1);
        obtenida.Conceptos[0].ValorPorDefecto!.Monto.Should().Be(1000000m);
    }

    public void Dispose() => _factory.Dispose();
}
