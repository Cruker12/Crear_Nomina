using System.Diagnostics;
using FluentAssertions;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Presentation.Commands;
using GeneradoNominaSystem.Presentation.ViewModels;
using Moq;

namespace GeneradoNominaSystem.Presentation.Tests;

public class ViewModelFeedbackTests
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
    public async Task EmpresaGuardar_CuandoServicioFalla_DeberiaMostrarErrorAmable()
    {
        var servicio = new Mock<IEmpresaService>();
        servicio.Setup(s => s.ListarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmpresaDto>());
        servicio.Setup(s => s.CrearAsync(It.IsAny<EmpresaDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ReglaNegocioException("Ya existe una empresa con NIT 123."));
        var vm = new EmpresaViewModel(servicio.Object);
        vm.Edicion = new EmpresaDto { Nit = "123" };

        vm.GuardarCommand.Execute(null);
        await EsperarMensajeAsync(() => vm.Mensaje);

        vm.EsError.Should().BeTrue();
        vm.Mensaje.Should().Contain("Ya existe una empresa");
        vm.Ocupado.Should().BeFalse();
    }

    [Fact]
    public async Task NominaExportar_CuandoServicioFalla_DeberiaMostrarErrorAmable()
    {
        var documentos = new Mock<IDocumentoService>();
        documentos.Setup(d => d.ExportarNominaAsync(It.IsAny<Guid>(), It.IsAny<Domain.Enums.FormatoExportacion>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ReglaNegocioException("Solo se pueden exportar nóminas calculadas."));
        var vm = new NominaViewModel(
            Mock.Of<INominaService>(),
            Mock.Of<IEmpresaService>(),
            Mock.Of<IEmpleadoService>(),
            Mock.Of<IPeriodoNominaService>(),
            Mock.Of<IConceptoNominaService>(),
            Mock.Of<IPlantillaNominaService>(),
            documentos.Object);
        vm.Seleccionada = new NominaDto { Id = Guid.NewGuid() };

        vm.ExportarPdfCommand.Execute(null);
        await EsperarMensajeAsync(() => vm.Mensaje);

        vm.EsError.Should().BeTrue();
        vm.Mensaje.Should().Contain("calculadas");
        vm.Ocupado.Should().BeFalse();
    }

    [Fact]
    public void RelayCommand_SinPredicado_DeberiaPoderEjecutar()
    {
        var ejecutado = false;
        var comando = new RelayCommand(_ => ejecutado = true);

        comando.CanExecute(null).Should().BeTrue();
        comando.Execute(null);

        ejecutado.Should().BeTrue();
    }

    [Fact]
    public void RelayCommand_SolicitarReevaluacion_NoDeberiaLanzar()
    {
        var accion = () => RelayCommand.SolicitarReevaluacion();

        accion.Should().NotThrow();
    }
}
