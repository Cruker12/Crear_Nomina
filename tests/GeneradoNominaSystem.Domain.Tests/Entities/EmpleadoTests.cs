using FluentAssertions;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Tests.Entities;

public class EmpleadoTests
{
    private static Empleado CrearEmpleado()
    {
        return new Empleado(
            Guid.NewGuid(),
            TipoDocumentoIdentidad.Cedula,
            "12345678",
            "Juan",
            "Pérez",
            new DateTime(2024, 1, 15),
            "Auxiliar",
            new Dinero(1500000m, "COP"));
    }

    [Fact]
    public void ActualizarDatosLaborales_DatosValidos_DeberiaActualizar()
    {
        var empleado = CrearEmpleado();

        empleado.ActualizarDatosLaborales("Coordinador", "Operaciones");

        empleado.Cargo.Should().Be("Coordinador");
        empleado.Departamento.Should().Be("Operaciones");
    }

    [Fact]
    public void ActualizarDatosLaborales_CargoVacio_DeberiaLanzarReglaNegocio()
    {
        var empleado = CrearEmpleado();

        var accion = () => empleado.ActualizarDatosLaborales("  ", null);

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void ActualizarSalario_Negativo_DeberiaLanzarReglaNegocio()
    {
        var empleado = CrearEmpleado();

        var accion = () => empleado.ActualizarSalario(new Dinero(-100m, "COP"));

        accion.Should().Throw<ReglaNegocioException>();
    }
}
