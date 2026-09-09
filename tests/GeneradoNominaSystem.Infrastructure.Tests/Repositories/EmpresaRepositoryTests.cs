using FluentAssertions;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.ValueObjects;
using GeneradoNominaSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GeneradoNominaSystem.Infrastructure.Tests.Repositories;

public sealed class EmpresaRepositoryTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();

    private static Empresa CrearEmpresa(string nit)
    {
        var direccion = new Direccion("Calle 1 # 2-3", "Medellín", "Antioquia");
        var fiscales = new DatosFiscales(nit, "Empresa Test S.A.S.");
        return new Empresa("Empresa Test S.A.S.", "Empresa Test", nit, direccion, fiscales);
    }

    [Fact]
    public async Task AgregarYObtener_EmpresaValida_DeberiaPersistir()
    {
        using var context = _factory.Crear();
        var repo = new EmpresaRepository(context);
        var empresa = CrearEmpresa("900123456");

        await repo.AgregarAsync(empresa);
        await context.SaveChangesAsync();

        var obtenida = await repo.ObtenerPorIdAsync(empresa.Id);
        obtenida.Should().NotBeNull();
        obtenida!.Nit.Should().Be("900123456");
        obtenida.Direccion.Ciudad.Should().Be("Medellín");
    }

    [Fact]
    public async Task ObtenerPorNit_NitExistente_DeberiaRetornarEmpresa()
    {
        using var context = _factory.Crear();
        var repo = new EmpresaRepository(context);
        await repo.AgregarAsync(CrearEmpresa("900999888"));
        await context.SaveChangesAsync();

        var obtenida = await repo.ObtenerPorNitAsync("900999888");

        obtenida.Should().NotBeNull();
    }

    [Fact]
    public async Task Agregar_NitDuplicado_DeberiaLanzarDbUpdateException()
    {
        using var context = _factory.Crear();
        var repo = new EmpresaRepository(context);
        await repo.AgregarAsync(CrearEmpresa("900111222"));
        await context.SaveChangesAsync();
        await repo.AgregarAsync(CrearEmpresa("900111222"));

        var accion = () => context.SaveChangesAsync();

        await accion.Should().ThrowAsync<DbUpdateException>();
    }

    public void Dispose() => _factory.Dispose();
}
