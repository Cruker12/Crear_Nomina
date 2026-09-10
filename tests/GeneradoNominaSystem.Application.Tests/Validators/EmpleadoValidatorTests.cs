using FluentAssertions;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Validators;

namespace GeneradoNominaSystem.Application.Tests.Validators;

public class EmpleadoValidatorTests
{
    private readonly EmpleadoValidator _sut = new();

    private static EmpleadoDto CrearDtoValido()
    {
        return new EmpleadoDto
        {
            EmpresaId = Guid.NewGuid(),
            NumeroDocumento = "12345678",
            Nombres = "Juan",
            Apellidos = "Pérez",
            FechaIngreso = new DateTime(2024, 1, 15),
            Cargo = "Auxiliar",
            SalarioBaseMonto = 1500000m,
            SalarioBaseMoneda = "COP",
        };
    }

    [Fact]
    public async Task Validate_DtoValido_DeberiaSerValido()
    {
        var resultado = await _sut.ValidateAsync(CrearDtoValido());

        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_DocumentoVacio_DeberiaFallar()
    {
        var dto = CrearDtoValido();
        dto.NumeroDocumento = string.Empty;

        var resultado = await _sut.ValidateAsync(dto);

        resultado.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_SalarioNegativo_DeberiaFallar()
    {
        var dto = CrearDtoValido();
        dto.SalarioBaseMonto = -100m;

        var resultado = await _sut.ValidateAsync(dto);

        resultado.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_FechaIngresoFutura_DeberiaFallar()
    {
        var dto = CrearDtoValido();
        dto.FechaIngreso = DateTime.Today.AddDays(1);

        var resultado = await _sut.ValidateAsync(dto);

        resultado.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("NN")]
    [InlineData("nn")]
    [InlineData(" Nn ")]
    public async Task Validate_EmailDesconocido_DeberiaSerValido(string email)
    {
        var dto = CrearDtoValido();
        dto.Email = email;

        var resultado = await _sut.ValidateAsync(dto);

        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_NombresDesconocido_DeberiaSerValido()
    {
        var dto = CrearDtoValido();
        dto.Nombres = "NN";
        dto.Apellidos = "NN";
        dto.Cargo = "NN";

        var resultado = await _sut.ValidateAsync(dto);

        resultado.IsValid.Should().BeTrue();
    }
}
