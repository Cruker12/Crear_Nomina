using System.Diagnostics;
using FluentAssertions;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Services;
using GeneradoNominaSystem.Presentation.ViewModels;
using Moq;

namespace GeneradoNominaSystem.Presentation.Tests;

public class ViewModelCoverageTests
{
    private static async Task EsperarMensajeAsync(Func<string> leerMensaje, int timeoutMs = 5000)
    {
        var sw = Stopwatch.StartNew();
        while (string.IsNullOrEmpty(leerMensaje()) && sw.ElapsedMilliseconds < timeoutMs)
        {
            await Task.Delay(50);
        }
    }

    [Fact]
    public async Task HistorialBuscar_DeberiaDelegarFiltrosAlServicio()
    {
        var historial = new Mock<IHistorialService>();
        historial.Setup(h => h.BuscarNominasAsync(It.IsAny<Guid>(), It.IsAny<string>(), null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NominaDto>());
        historial.Setup(h => h.BuscarCotizacionesAsync(It.IsAny<Guid>(), It.IsAny<string>(), null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CotizacionDto>());
        historial.Setup(h => h.ListarDocumentosAsync(It.IsAny<Guid>(), null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DocumentoDto>());
        var empresas = new Mock<IEmpresaService>();
        empresas.Setup(e => e.ListarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmpresaDto>());
        var vm = new HistorialViewModel(historial.Object, empresas.Object, Mock.Of<INotificadorDocumentos>());
        vm.EmpresaSeleccionada = new EmpresaDto { Id = Guid.NewGuid(), NombreComercial = "E" };
        vm.Texto = "juan";

        vm.BuscarCommand.Execute(null);
        await EsperarMensajeAsync(() => vm.Mensaje);

        historial.Verify(h => h.BuscarNominasAsync(It.IsAny<Guid>(), "juan", null, null, null, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        vm.EsError.Should().BeFalse();
    }

    [Fact]
    public async Task HistorialNotificacion_AlGenerarDocumento_DeberiaRefrescar()
    {
        var historial = new Mock<IHistorialService>();
        historial.Setup(h => h.BuscarNominasAsync(It.IsAny<Guid>(), It.IsAny<string>(), null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NominaDto>());
        historial.Setup(h => h.BuscarCotizacionesAsync(It.IsAny<Guid>(), It.IsAny<string>(), null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CotizacionDto>());
        historial.Setup(h => h.ListarDocumentosAsync(It.IsAny<Guid>(), null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DocumentoDto>());
        var empresas = new Mock<IEmpresaService>();
        empresas.Setup(e => e.ListarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmpresaDto>());
        var notificador = new Mock<INotificadorDocumentos>();
        var vm = new HistorialViewModel(historial.Object, empresas.Object, notificador.Object);
        vm.EmpresaSeleccionada = new EmpresaDto { Id = Guid.NewGuid(), NombreComercial = "E" };
        await EsperarMensajeAsync(() => vm.Mensaje);

        notificador.Raise(
            n => n.DocumentoGenerado += null,
            new DocumentoGeneradoArgs(Guid.NewGuid(), "ruta.pdf"));

        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < 5000)
        {
            try
            {
                historial.Verify(
                    h => h.ListarDocumentosAsync(It.IsAny<Guid>(), null, null, null, null, It.IsAny<CancellationToken>()),
                    Times.AtLeast(2));
                break;
            }
            catch
            {
                await Task.Delay(50);
            }
        }

        historial.Verify(
            h => h.ListarDocumentosAsync(It.IsAny<Guid>(), null, null, null, null, It.IsAny<CancellationToken>()),
            Times.AtLeast(2));
        vm.EsError.Should().BeFalse();
    }

    [Fact]
    public async Task EmpleadoGuardar_FlujoOk_DeberiaLimpiarYConfirmar()
    {
        var empleados = new Mock<IEmpleadoService>();
        empleados.Setup(e => e.CrearAsync(It.IsAny<EmpleadoDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmpleadoDto dto, CancellationToken _) => dto);
        empleados.Setup(e => e.ListarPorEmpresaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmpleadoDto>());
        var empresas = new Mock<IEmpresaService>();
        var empresa = new EmpresaDto { Id = Guid.NewGuid(), NombreComercial = "E" };
        empresas.Setup(e => e.ListarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmpresaDto> { empresa });
        var vm = new EmpleadoViewModel(empleados.Object, empresas.Object);
        vm.EmpresaSeleccionada = empresa;
        vm.Edicion = new EmpleadoDto
        {
            NumeroDocumento = "123",
            Nombres = "Ana",
            Apellidos = "Luz",
            FechaIngreso = new DateTime(2024, 1, 15),
            Cargo = "Aux",
            SalarioBaseMonto = 1000000m,
        };

        vm.GuardarCommand.Execute(null);
        await EsperarMensajeAsync(() => vm.Mensaje);

        vm.EsError.Should().BeFalse();
        vm.Mensaje.Should().Contain("creado");
        empleados.Verify(e => e.CrearAsync(It.Is<EmpleadoDto>(d => d.EmpresaId == empresa.Id), It.IsAny<CancellationToken>()), Times.Once);
    }
}
