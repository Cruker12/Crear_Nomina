using FluentAssertions;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Validators;

namespace GeneradoNominaSystem.Application.Tests.Validators;

public class CotizacionValidatorTests
{
    private readonly CotizacionValidator _sut = new();
    private readonly DetalleCotizacionValidator _detalleSut = new();

    private static CotizacionDto CrearDtoValido()
    {
        return new CotizacionDto
        {
            EmpresaId = Guid.NewGuid(),
            ClienteNombre = "Cliente Test",
            FechaEmision = new DateTime(2026, 3, 1),
            FechaVigencia = new DateTime(2026, 3, 31),
            Moneda = "COP",
        };
    }

    [Fact]
    public async Task Validate_DtoValido_DeberiaSerValido()
    {
        var resultado = await _sut.ValidateAsync(CrearDtoValido());

        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_VigenciaAnteriorAEmision_DeberiaFallar()
    {
        var dto = CrearDtoValido();
        dto.FechaVigencia = dto.FechaEmision.AddDays(-1);

        var resultado = await _sut.ValidateAsync(dto);

        resultado.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_ClienteDesconocido_DeberiaSerValido()
    {
        var dto = CrearDtoValido();
        dto.ClienteNombre = "NN";
        dto.ClienteEmail = "nn";

        var resultado = await _sut.ValidateAsync(dto);

        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateDetalle_CantidadCero_DeberiaFallar()
    {
        var dto = new DetalleCotizacionDto
        {
            Descripcion = "Servicio",
            Cantidad = 0m,
            PrecioUnitarioMonto = 100000m,
            PrecioUnitarioMoneda = "COP",
        };

        var resultado = await _detalleSut.ValidateAsync(dto);

        resultado.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateDetalle_DescuentoMayor100_DeberiaFallar()
    {
        var dto = new DetalleCotizacionDto
        {
            Descripcion = "Servicio",
            Cantidad = 1m,
            PrecioUnitarioMonto = 100000m,
            PrecioUnitarioMoneda = "COP",
            DescuentoPorcentaje = 150m,
        };

        var resultado = await _detalleSut.ValidateAsync(dto);

        resultado.IsValid.Should().BeFalse();
    }
}
