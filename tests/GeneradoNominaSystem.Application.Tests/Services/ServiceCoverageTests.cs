using FluentAssertions;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Services;
using GeneradoNominaSystem.Application.Validators;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Domain.Interfaces.Services;
using GeneradoNominaSystem.Domain.ValueObjects;
using Moq;

namespace GeneradoNominaSystem.Application.Tests.Services;

public class ServiceCoverageTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Guid _empresaId = Guid.NewGuid();

    private static Empresa CrearEmpresa()
    {
        return new Empresa(
            "Empresa Test S.A.S.",
            "Empresa Test",
            "900123456",
            new Direccion("Calle 1", "Bogotá", "Cundinamarca"),
            new DatosFiscales("900123456", "Empresa Test S.A.S."));
    }

    private static Empleado CrearEmpleado(Guid empresaId)
    {
        return new Empleado(
            empresaId,
            TipoDocumentoIdentidad.Cedula,
            "12345678",
            "Juan",
            "Pérez",
            new DateTime(2024, 1, 15),
            "Auxiliar",
            new Dinero(1500000m, "COP"));
    }

    private static EmpresaDto CrearEmpresaDto()
    {
        return new EmpresaDto
        {
            RazonSocial = "Empresa Test S.A.S.",
            NombreComercial = "Empresa Test",
            Nit = "900123456",
            DireccionCompleta = "Calle 1",
            Ciudad = "Bogotá",
            Departamento = "Cundinamarca",
        };
    }

    [Fact]
    public async Task EmpresaService_ActualizarYDesactivar_DeberiaGuardar()
    {
        var repo = new Mock<IEmpresaRepository>();
        var empresa = CrearEmpresa();
        repo.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(empresa);
        repo.Setup(r => r.ObtenerPorNitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Empresa?)null);
        var sut = new EmpresaService(repo.Object, Mock.Of<IEmpleadoRepository>(), _uow.Object, new EmpresaValidator());

        var dto = CrearEmpresaDto();
        dto.Id = empresa.Id;
        dto.NombreComercial = "Nuevo Nombre";
        var actualizada = await sut.ActualizarAsync(dto);
        await sut.DesactivarAsync(empresa.Id);

        actualizada.NombreComercial.Should().Be("Nuevo Nombre");
        empresa.Activo.Should().BeFalse();

        await sut.ActivarAsync(empresa.Id);
        empresa.Activo.Should().BeTrue();

        _uow.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task EmpleadoService_Actualizar_DeberiaGuardar()
    {
        var repo = new Mock<IEmpleadoRepository>();
        var empleado = CrearEmpleado(_empresaId);
        repo.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(empleado);
        repo.Setup(r => r.ObtenerPorDocumentoAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Empleado?)null);
        var sut = new EmpleadoService(repo.Object, Mock.Of<IEmpresaRepository>(), _uow.Object, new EmpleadoValidator());

        var dto = new EmpleadoDto
        {
            Id = empleado.Id,
            EmpresaId = _empresaId,
            NumeroDocumento = "12345678",
            Nombres = "Juan",
            Apellidos = "Pérez",
            FechaIngreso = new DateTime(2024, 1, 15),
            Cargo = "Coordinador",
            SalarioBaseMonto = 2000000m,
            SalarioBaseMoneda = "COP",
        };

        var resultado = await sut.ActualizarAsync(dto);

        resultado.Cargo.Should().Be("Coordinador");
        resultado.SalarioBaseMonto.Should().Be(2000000m);
    }

    [Fact]
    public async Task ConceptoService_Activar_DeberiaGuardar()
    {
        var repo = new Mock<IConceptoNominaRepository>();
        var concepto = new ConceptoNomina("Bono", TipoConcepto.Devengo, 5, _empresaId, valorFijo: new Dinero(0m, "COP"));
        concepto.Desactivar();
        repo.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(concepto);
        var sut = new ConceptoNominaService(repo.Object, _uow.Object, new ConceptoNominaValidator());

        await sut.ActivarAsync(concepto.Id);

        concepto.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task PeriodoService_Desactivar_DeberiaGuardar()
    {
        var repo = new Mock<IPeriodoNominaRepository>();
        var periodo = new PeriodoNomina(_empresaId, "Abril 2026", TipoPeriodo.Mensual, new DateTime(2026, 4, 1), new DateTime(2026, 4, 30));
        repo.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(periodo);
        var sut = new PeriodoNominaService(repo.Object, _uow.Object, new PeriodoNominaValidator());

        await sut.DesactivarAsync(periodo.Id);

        periodo.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task ProductoService_Desactivar_DeberiaGuardar()
    {
        var repo = new Mock<IProductoServicioRepository>();
        var producto = new ProductoServicio(_empresaId, "Consultoría", new Dinero(100000m, "COP"));
        repo.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(producto);
        var sut = new ProductoServicioService(repo.Object, _uow.Object, new ProductoServicioValidator());

        await sut.DesactivarAsync(producto.Id);

        producto.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task PlantillaNominaService_DesactivarYObtener_DeberiaFuncionar()
    {
        var repo = new Mock<IPlantillaNominaRepository>();
        var plantilla = new PlantillaNomina(_empresaId, "Base");
        repo.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(plantilla);
        repo.Setup(r => r.ObtenerConConceptosAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(plantilla);
        var sut = new PlantillaNominaService(
            repo.Object, Mock.Of<IConceptoNominaRepository>(), _uow.Object, new PlantillaNominaValidator());

        await sut.DesactivarAsync(plantilla.Id);
        var obtenida = await sut.ObtenerPorIdAsync(plantilla.Id);

        plantilla.Activo.Should().BeFalse();
        obtenida.Should().NotBeNull();
    }

    [Fact]
    public async Task PlantillaCotizacionService_Desactivar_DeberiaGuardar()
    {
        var repo = new Mock<IPlantillaCotizacionRepository>();
        var plantilla = new PlantillaCotizacion(_empresaId, "Formal");
        repo.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(plantilla);
        var sut = new PlantillaCotizacionService(repo.Object, _uow.Object, new PlantillaCotizacionValidator());

        await sut.DesactivarAsync(plantilla.Id);

        plantilla.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task NominaService_PagarYAnular_DeberiaTransicionar()
    {
        var nominas = new Mock<INominaRepository>();
        var aprobada = new Nomina(_empresaId, Guid.NewGuid(), Guid.NewGuid());
        aprobada.AplicarTotales(new Dinero(1000m, "COP"), Dinero.Cero());
        aprobada.MarcarCalculada();
        aprobada.Aprobar();
        var paraAnular = new Nomina(_empresaId, Guid.NewGuid(), Guid.NewGuid());
        paraAnular.AplicarTotales(new Dinero(1000m, "COP"), Dinero.Cero());
        paraAnular.MarcarCalculada();
        nominas.Setup(r => r.ObtenerPorIdAsync(aprobada.Id, It.IsAny<CancellationToken>())).ReturnsAsync(aprobada);
        nominas.Setup(r => r.ObtenerPorIdAsync(paraAnular.Id, It.IsAny<CancellationToken>())).ReturnsAsync(paraAnular);
        var sut = new NominaService(
            nominas.Object, Mock.Of<IEmpleadoRepository>(), Mock.Of<IPeriodoNominaRepository>(),
            Mock.Of<IConceptoNominaRepository>(), Mock.Of<IPlantillaNominaRepository>(),
            _uow.Object, new DetalleNominaValidator());

        await sut.MarcarPagadaAsync(aprobada.Id);
        await sut.AnularAsync(paraAnular.Id, "Error de digitación");

        aprobada.Estado.Should().Be(EstadoNomina.Pagada);
        paraAnular.Estado.Should().Be(EstadoNomina.Anulada);
    }

    [Fact]
    public async Task CotizacionService_CrearConPlantilla_DeberiaAsignarla()
    {
        var cotizaciones = new Mock<ICotizacionRepository>();
        var plantillas = new Mock<IPlantillaCotizacionRepository>();
        var plantilla = new PlantillaCotizacion(_empresaId, "Formal");
        plantillas.Setup(r => r.ObtenerPorIdAsync(plantilla.Id, It.IsAny<CancellationToken>())).ReturnsAsync(plantilla);
        cotizaciones.Setup(r => r.ListarPorEmpresaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Cotizacion>());
        var sut = new CotizacionService(
            cotizaciones.Object, Mock.Of<IProductoServicioRepository>(), plantillas.Object,
            _uow.Object, new Domain.Services.ServicioNumeracion(),
            new CotizacionValidator(), new DetalleCotizacionValidator());

        var resultado = await sut.CrearAsync(new CotizacionDto
        {
            EmpresaId = _empresaId,
            ClienteNombre = "Cliente",
            FechaEmision = new DateTime(2026, 3, 1),
            FechaVigencia = new DateTime(2026, 3, 31),
            Moneda = "COP",
            PlantillaCotizacionId = plantilla.Id,
        });

        resultado.PlantillaCotizacionId.Should().Be(plantilla.Id);
    }

    [Fact]
    public async Task DocumentoService_ListarPorEmpresaYReferencia_DeberiaMapear()
    {
        var documentos = new Mock<IDocumentoRepository>();
        var lista = new List<Documento>
        {
            new(_empresaId, TipoDocumentoSistema.Nomina, Guid.NewGuid(), "NOM-2026-0001", FormatoExportacion.Pdf, "ruta.pdf", 1234, "Sistema"),
        };
        documentos.Setup(r => r.ListarPorEmpresaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(lista);
        documentos.Setup(r => r.ListarPorReferenciaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(lista);
        var sut = new DocumentoService(
            Mock.Of<INominaRepository>(), Mock.Of<ICotizacionRepository>(), Mock.Of<IProductoServicioRepository>(),
            Mock.Of<IEmpresaRepository>(), Mock.Of<IEmpleadoRepository>(), Mock.Of<IPeriodoNominaRepository>(),
            Mock.Of<IConceptoNominaRepository>(), documentos.Object, _uow.Object,
            Mock.Of<IServicioNumeracion>(), new List<IExportadorDocumento>());

        var porEmpresa = await sut.ListarPorEmpresaAsync(_empresaId);
        var porReferencia = await sut.ListarPorReferenciaAsync(Guid.NewGuid());

        porEmpresa.Should().HaveCount(1);
        porReferencia.Should().HaveCount(1);
        porEmpresa[0].TamanoBytes.Should().Be(1234);
    }
}
