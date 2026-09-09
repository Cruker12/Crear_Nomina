using FluentAssertions;
using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Services;
using GeneradoNominaSystem.Application.Validators;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Domain.ValueObjects;
using Moq;

namespace GeneradoNominaSystem.Application.Tests.Services;

public class EmpleadoServiceTests
{
    private readonly Mock<IEmpleadoRepository> _empleados = new();
    private readonly Mock<IEmpresaRepository> _empresas = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private static Empresa CrearEmpresa()
    {
        return new Empresa(
            "Empresa Test S.A.S.",
            "Empresa Test",
            "900123456",
            new Direccion("Calle 1", "Bogotá", "Cundinamarca"),
            new DatosFiscales("900123456", "Empresa Test S.A.S."));
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

    private static EmpleadoDto CrearDtoValido(Guid empresaId)
    {
        return new EmpleadoDto
        {
            EmpresaId = empresaId,
            NumeroDocumento = "12345678",
            Nombres = "Juan",
            Apellidos = "Pérez",
            FechaIngreso = new DateTime(2024, 1, 15),
            Cargo = "Auxiliar",
            SalarioBaseMonto = 1500000m,
            SalarioBaseMoneda = "COP",
        };
    }

    private EmpleadoService CrearSut()
    {
        return new EmpleadoService(_empleados.Object, _empresas.Object, _uow.Object, new EmpleadoValidator());
    }

    [Fact]
    public async Task CrearAsync_DtoValido_DeberiaPersistir()
    {
        var empresa = CrearEmpresa();
        _empresas.Setup(r => r.ObtenerPorIdAsync(empresa.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresa);
        _empleados.Setup(r => r.ObtenerPorDocumentoAsync(empresa.Id, "12345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Empleado?)null);
        var sut = CrearSut();

        var resultado = await sut.CrearAsync(CrearDtoValido(empresa.Id));

        resultado.NumeroDocumento.Should().Be("12345678");
        _empleados.Verify(r => r.AgregarAsync(It.IsAny<Empleado>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearAsync_DocumentoDuplicadoMismaEmpresa_DeberiaLanzarReglaNegocio()
    {
        var empresa = CrearEmpresa();
        _empresas.Setup(r => r.ObtenerPorIdAsync(empresa.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(empresa);
        _empleados.Setup(r => r.ObtenerPorDocumentoAsync(empresa.Id, "12345678", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CrearEmpleado(empresa.Id, "12345678"));
        var sut = CrearSut();

        var accion = () => sut.CrearAsync(CrearDtoValido(empresa.Id));

        await accion.Should().ThrowAsync<ReglaNegocioException>();
        _uow.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CrearAsync_EmpresaInexistente_DeberiaLanzarReglaNegocio()
    {
        _empresas.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Empresa?)null);
        var sut = CrearSut();

        var accion = () => sut.CrearAsync(CrearDtoValido(Guid.NewGuid()));

        await accion.Should().ThrowAsync<ReglaNegocioException>();
    }

    [Fact]
    public async Task CrearAsync_SalarioNegativo_DeberiaLanzarValidationException()
    {
        var empresa = CrearEmpresa();
        var dto = CrearDtoValido(empresa.Id);
        dto.SalarioBaseMonto = -500m;
        var sut = CrearSut();

        var accion = () => sut.CrearAsync(dto);

        await accion.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CambiarEstado_EmpleadoExistente_DeberiaActualizar()
    {
        var empresa = CrearEmpresa();
        var empleado = CrearEmpleado(empresa.Id, "12345678");
        _empleados.Setup(r => r.ObtenerPorIdAsync(empleado.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(empleado);
        var sut = CrearSut();

        await sut.CambiarEstadoAsync(empleado.Id, EstadoEmpleado.Inactivo);

        empleado.Estado.Should().Be(EstadoEmpleado.Inactivo);
        _uow.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
