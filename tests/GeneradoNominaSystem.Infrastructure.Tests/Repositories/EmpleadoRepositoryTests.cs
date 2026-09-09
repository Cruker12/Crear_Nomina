using FluentAssertions;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.ValueObjects;
using GeneradoNominaSystem.Infrastructure.Repositories;

namespace GeneradoNominaSystem.Infrastructure.Tests.Repositories;

public sealed class EmpleadoRepositoryTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();

    private static Empresa CrearEmpresa()
    {
        return new Empresa(
            "Empresa Test S.A.S.",
            "Empresa Test",
            $"900{Random.Shared.Next(100000, 999999)}",
            new Direccion("Calle 1 # 2-3", "Bogotá", "Cundinamarca"),
            new DatosFiscales("900000001", "Empresa Test S.A.S."));
    }

    private static Empleado CrearEmpleado(Guid empresaId, string documento)
    {
        return new Empleado(
            empresaId,
            TipoDocumentoIdentidad.Cedula,
            documento,
            "Juan",
            "Pérez",
            new DateTime(2024, 1, 15),
            "Auxiliar",
            new Dinero(1500000m, "COP"));
    }

    [Fact]
    public async Task AgregarYListarPorEmpresa_DeberiaPersistirYFiltrar()
    {
        using var context = _factory.Crear();
        var empresaRepo = new EmpresaRepository(context);
        var empleadoRepo = new EmpleadoRepository(context);

        var empresa = CrearEmpresa();
        await empresaRepo.AgregarAsync(empresa);
        await context.SaveChangesAsync();

        await empleadoRepo.AgregarAsync(CrearEmpleado(empresa.Id, "12345678"));
        await empleadoRepo.AgregarAsync(CrearEmpleado(empresa.Id, "87654321"));
        await context.SaveChangesAsync();

        var empleados = await empleadoRepo.ListarPorEmpresaAsync(empresa.Id);
        empleados.Should().HaveCount(2);

        var obtenido = await empleadoRepo.ObtenerPorDocumentoAsync(empresa.Id, "12345678");
        obtenido.Should().NotBeNull();
        obtenido!.NombreCompleto.Should().Be("Juan Pérez");
        obtenido.SalarioBase.Monto.Should().Be(1500000m);
    }

    public void Dispose() => _factory.Dispose();
}
