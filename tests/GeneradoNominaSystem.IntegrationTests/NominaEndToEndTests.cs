using FluentAssertions;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Services;
using GeneradoNominaSystem.Application.Validators;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Interfaces.Services;
using GeneradoNominaSystem.Domain.Services;
using GeneradoNominaSystem.Infrastructure.Data;
using GeneradoNominaSystem.Infrastructure.Exportadores;
using GeneradoNominaSystem.Infrastructure.Repositories;
using GeneradoNominaSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GeneradoNominaSystem.IntegrationTests;

public sealed class NominaEndToEndTests : IDisposable
{
    private readonly string _rutaBd = Path.Combine(Path.GetTempPath(), $"gns-e2e-{Guid.NewGuid():N}.db");
    private readonly string _carpetaDocs = Path.Combine(Path.GetTempPath(), $"gns-e2e-docs-{Guid.NewGuid():N}");

    private AppDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={_rutaBd}")
            .Options;

        var context = new AppDbContext(options);
        context.Database.Migrate();
        return context;
    }

    [Fact]
    public async Task FlujoCompleto_EmpresaEmpleadoNominaPdfExcel_DeberiaFuncionar()
    {
        Directory.CreateDirectory(_carpetaDocs);
        using var context = CrearContexto();

        var uow = new EfUnitOfWork(context);
        var empresaRepo = new EmpresaRepository(context);
        var empleadoRepo = new EmpleadoRepository(context);
        var periodoRepo = new PeriodoNominaRepository(context);
        var conceptoRepo = new ConceptoNominaRepository(context);
        var nominaRepo = new NominaRepository(context);
        var plantillaRepo = new PlantillaNominaRepository(context);
        var documentoRepo = new DocumentoRepository(context);
        var cotizacionRepo = new CotizacionRepository(context);
        var productoRepo = new ProductoServicioRepository(context);

        var empresaService = new EmpresaService(empresaRepo, empleadoRepo, uow, new EmpresaValidator());
        var empleadoService = new EmpleadoService(empleadoRepo, empresaRepo, nominaRepo, uow, new EmpleadoValidator());
        var periodoService = new PeriodoNominaService(periodoRepo, nominaRepo, uow, new PeriodoNominaValidator());
        var conceptoService = new ConceptoNominaService(conceptoRepo, uow, new ConceptoNominaValidator());
        var nominaService = new NominaService(
            nominaRepo, empleadoRepo, periodoRepo, conceptoRepo, plantillaRepo, uow, new DetalleNominaValidator());
        var documentoService = new DocumentoService(
            nominaRepo, cotizacionRepo, productoRepo, empresaRepo, empleadoRepo,
            periodoRepo, conceptoRepo, documentoRepo, uow,
            new ServicioNumeracion(),
            new List<IExportadorDocumento> { new ExportadorNominaPdf(), new ExportadorNominaExcel() },
            new NotificadorDocumentos());

        var empresa = await empresaService.CrearAsync(new EmpresaDto
        {
            RazonSocial = "Empresa E2E S.A.S.",
            NombreComercial = "Empresa E2E",
            Nit = "900123999",
            DireccionCompleta = "Calle 1",
            Ciudad = "Bogotá",
            Departamento = "Cundinamarca",
        });

        var empleado = await empleadoService.CrearAsync(new EmpleadoDto
        {
            EmpresaId = empresa.Id,
            NumeroDocumento = "12345678",
            Nombres = "Juan",
            Apellidos = "Pérez",
            FechaIngreso = new DateTime(2024, 1, 15),
            Cargo = "Auxiliar",
            SalarioBaseMonto = 2000000m,
            SalarioBaseMoneda = "COP",
        });

        var periodo = await periodoService.CrearAsync(new PeriodoNominaDto
        {
            EmpresaId = empresa.Id,
            Nombre = "Marzo 2026",
            Tipo = TipoPeriodo.Mensual,
            FechaInicio = new DateTime(2026, 3, 1),
            FechaFin = new DateTime(2026, 3, 31),
        });

        var salario = await conceptoService.CrearAsync(new ConceptoNominaDto
        {
            EmpresaId = empresa.Id,
            Nombre = "Salario",
            Tipo = TipoConcepto.Devengo,
            Orden = 1,
            ValorFijoMonto = 2000000m,
        });

        var salud = await conceptoService.CrearAsync(new ConceptoNominaDto
        {
            EmpresaId = empresa.Id,
            Nombre = "Salud",
            Tipo = TipoConcepto.Deduccion,
            Orden = 2,
            EsPorcentaje = true,
            PorcentajeBase = 4m,
        });

        var nomina = await nominaService.CrearAsync(empresa.Id, empleado.Id, periodo.Id);
        await nominaService.AgregarDetalleAsync(nomina.Id, salario.Id, 2000000m, "COP");
        await nominaService.AgregarDetalleAsync(nomina.Id, salud.Id, 0m, "COP");
        var calculada = await nominaService.CalcularAsync(nomina.Id, 2000000m);

        calculada.Estado.Should().Be(EstadoNomina.Calculada);
        calculada.SubtotalDevengos.Should().Be(2000000m);
        calculada.SubtotalDeducciones.Should().Be(80000m);
        calculada.TotalNeto.Should().Be(1920000m);

        await nominaService.AprobarAsync(nomina.Id);

        var pdf = await documentoService.ExportarNominaAsync(nomina.Id, FormatoExportacion.Pdf, Path.Combine(_carpetaDocs, "nomina.pdf"));
        var excel = await documentoService.ExportarNominaAsync(nomina.Id, FormatoExportacion.Excel, Path.Combine(_carpetaDocs, "nomina.xlsx"));

        File.Exists(pdf.RutaArchivo).Should().BeTrue();
        File.Exists(excel.RutaArchivo).Should().BeTrue();
        pdf.TamanoBytes.Should().BeGreaterThan(0);
        excel.TamanoBytes.Should().BeGreaterThan(0);

        var documentos = await documentoService.ListarPorReferenciaAsync(nomina.Id);
        documentos.Should().HaveCount(2);
    }

    [Fact]
    public async Task Plantilla_AgregarYQuitarConceptoReal_DeberiaPersistir()
    {
        using var context = CrearContexto();
        var uow = new EfUnitOfWork(context);
        var empresaRepo = new EmpresaRepository(context);
        var empleadoRepo = new EmpleadoRepository(context);
        var conceptoRepo = new ConceptoNominaRepository(context);
        var nominaRepo = new NominaRepository(context);
        var plantillaRepo = new PlantillaNominaRepository(context);

        var empresaService = new EmpresaService(empresaRepo, empleadoRepo, uow, new EmpresaValidator());
        var plantillaService = new PlantillaNominaService(plantillaRepo, conceptoRepo, nominaRepo, uow, new PlantillaNominaValidator());
        var conceptoService = new ConceptoNominaService(conceptoRepo, uow, new ConceptoNominaValidator());

        var empresa = await empresaService.CrearAsync(new EmpresaDto
        {
            RazonSocial = "Empresa E2E S.A.S.",
            NombreComercial = "Empresa E2E",
            Nit = "900123999",
            DireccionCompleta = "Calle 1",
            Ciudad = "Bogotá",
            Departamento = "Cundinamarca",
        });

        var concepto = await conceptoService.CrearAsync(new ConceptoNominaDto
        {
            EmpresaId = empresa.Id,
            Nombre = "Salario",
            Tipo = TipoConcepto.Devengo,
            Orden = 1,
            ValorFijoMonto = 2000000m,
        });

        var plantilla = await plantillaService.CrearAsync(new PlantillaNominaDto
        {
            EmpresaId = empresa.Id,
            Nombre = "Base",
        });

        var conConcepto = await plantillaService.AgregarConceptoAsync(plantilla.Id, concepto.Id, 1, true, 1500000m);
        conConcepto.Conceptos.Should().HaveCount(1);

        await plantillaService.QuitarConceptoAsync(plantilla.Id, concepto.Id);
        var sinConcepto = await plantillaService.ObtenerPorIdAsync(plantilla.Id);
        sinConcepto!.Conceptos.Should().BeEmpty();
    }

    [Fact]
    public async Task Cotizacion_AgregarYQuitarDetalleReal_DeberiaRecalcular()
    {
        using var context = CrearContexto();
        var uow = new EfUnitOfWork(context);
        var empresaRepo = new EmpresaRepository(context);
        var empleadoRepo = new EmpleadoRepository(context);
        var cotizacionRepo = new CotizacionRepository(context);
        var productoRepo = new ProductoServicioRepository(context);
        var plantillaCotRepo = new PlantillaCotizacionRepository(context);

        var empresaService = new EmpresaService(empresaRepo, empleadoRepo, uow, new EmpresaValidator());
        var cotizacionService = new CotizacionService(
            cotizacionRepo, productoRepo, plantillaCotRepo, uow,
            new ServicioNumeracion(), new CotizacionValidator(), new DetalleCotizacionValidator());

        var empresa = await empresaService.CrearAsync(new EmpresaDto
        {
            RazonSocial = "Empresa E2E S.A.S.",
            NombreComercial = "Empresa E2E",
            Nit = "900123999",
            DireccionCompleta = "Calle 1",
            Ciudad = "Bogotá",
            Departamento = "Cundinamarca",
        });

        var cotizacion = await cotizacionService.CrearAsync(new CotizacionDto
        {
            EmpresaId = empresa.Id,
            ClienteNombre = "Cliente Test",
            FechaEmision = new DateTime(2026, 3, 1),
            FechaVigencia = new DateTime(2026, 3, 31),
            Moneda = "COP",
        });

        var conDetalle = await cotizacionService.AgregarDetalleAsync(cotizacion.Id, "Servicio", 2m, 100000m, "COP");
        conDetalle.Detalles.Should().HaveCount(1);
        conDetalle.Subtotal.Should().Be(200000m);

        await cotizacionService.QuitarDetalleAsync(cotizacion.Id, conDetalle.Detalles[0].Id);
        var sinDetalle = await cotizacionService.ObtenerPorIdAsync(cotizacion.Id);
        sinDetalle!.Detalles.Should().BeEmpty();
        sinDetalle.Subtotal.Should().Be(0m);
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(_rutaBd))
            {
                File.Delete(_rutaBd);
            }

            if (Directory.Exists(_carpetaDocs))
            {
                Directory.Delete(_carpetaDocs, recursive: true);
            }
        }
        catch
        {
        }
    }
}
