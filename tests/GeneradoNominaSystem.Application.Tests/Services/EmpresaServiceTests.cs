using FluentAssertions;
using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Services;
using GeneradoNominaSystem.Application.Validators;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Domain.ValueObjects;
using Moq;

namespace GeneradoNominaSystem.Application.Tests.Services;

public class EmpresaServiceTests
{
    private readonly Mock<IEmpresaRepository> _empresas = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private EmpresaService CrearSut()
    {
        return new EmpresaService(_empresas.Object, _uow.Object, new EmpresaValidator());
    }

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

    private static Empresa CrearEntidad()
    {
        return new Empresa(
            "Empresa Test S.A.S.",
            "Empresa Test",
            "900123456",
            new Direccion("Calle 1 # 2-3", "Medellín", "Antioquia"),
            new DatosFiscales("900123456", "Empresa Test S.A.S."));
    }

    [Fact]
    public async Task CrearAsync_DtoValido_DeberiaPersistirYRetornarDto()
    {
        _empresas.Setup(r => r.ObtenerPorNitAsync("900123456", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Empresa?)null);
        var sut = CrearSut();

        var resultado = await sut.CrearAsync(CrearDtoValido());

        resultado.Nit.Should().Be("900123456");
        _empresas.Verify(r => r.AgregarAsync(It.IsAny<Empresa>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearAsync_NitDuplicado_DeberiaLanzarReglaNegocio()
    {
        _empresas.Setup(r => r.ObtenerPorNitAsync("900123456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CrearEntidad());
        var sut = CrearSut();

        var accion = () => sut.CrearAsync(CrearDtoValido());

        await accion.Should().ThrowAsync<ReglaNegocioException>();
        _uow.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CrearAsync_NitVacio_DeberiaLanzarValidationException()
    {
        var dto = CrearDtoValido();
        dto.Nit = string.Empty;
        var sut = CrearSut();

        var accion = () => sut.CrearAsync(dto);

        await accion.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ActualizarAsync_EmpresaInexistente_DeberiaLanzarReglaNegocio()
    {
        _empresas.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Empresa?)null);
        var sut = CrearSut();
        var dto = CrearDtoValido();
        dto.Id = Guid.NewGuid();

        var accion = () => sut.ActualizarAsync(dto);

        await accion.Should().ThrowAsync<ReglaNegocioException>();
    }
}
