using FluentAssertions;
using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Validators;

namespace GeneradoNominaSystem.Application.Tests.Validators;

public class EmpresaValidatorTests
{
    private readonly EmpresaValidator _sut = new();

    private static EmpresaDto CrearDtoValido()
    {
        return new EmpresaDto
        {
            RazonSocial = "Empresa Test S.A.S.",
            NombreComercial = "Empresa Test",
            Nit = "900123456",
            DireccionCompleta = "Calle 1 # 2-3",
            Ciudad = "Medellín",
            Departamento = "Antioquia",
        };
    }

    [Fact]
    public async Task Validate_DtoValido_DeberiaSerValido()
    {
        var resultado = await _sut.ValidateAsync(CrearDtoValido());

        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_NitVacio_DeberiaFallar()
    {
        var dto = CrearDtoValido();
        dto.Nit = string.Empty;

        var resultado = await _sut.ValidateAsync(dto);

        resultado.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_EmailInvalido_DeberiaFallar()
    {
        var dto = CrearDtoValido();
        dto.Email = "no-es-email";

        var resultado = await _sut.ValidateAsync(dto);

        resultado.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("NN")]
    [InlineData("nn")]
    public async Task Validate_EmailDesconocido_DeberiaSerValido(string email)
    {
        var dto = CrearDtoValido();
        dto.Email = email;

        var resultado = await _sut.ValidateAsync(dto);

        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_CamposDesconocidos_DeberiaSerValido()
    {
        var dto = CrearDtoValido();
        dto.RazonSocial = "NN";
        dto.NombreComercial = "NN";
        dto.DireccionCompleta = "NN";
        dto.Ciudad = "NN";
        dto.Departamento = "NN";

        var resultado = await _sut.ValidateAsync(dto);

        resultado.IsValid.Should().BeTrue();
    }
}
