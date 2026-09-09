using FluentAssertions;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Services;

namespace GeneradoNominaSystem.Domain.Tests.Services;

public class ServicioNumeracionTests
{
    private readonly ServicioNumeracion _sut = new();

    [Fact]
    public void GenerarNumero_DatosValidos_DeberiaRetornarFormatoSecuencial()
    {
        var resultado = _sut.GenerarNumero("COT", 2026, 45);

        resultado.Should().Be("COT-2026-0045");
    }

    [Fact]
    public void GenerarNumero_SecuenciaCero_DeberiaLanzarReglaNegocio()
    {
        var accion = () => _sut.GenerarNumero("NOM", 2026, 0);

        accion.Should().Throw<ReglaNegocioException>();
    }
}
