using GeneradoNominaSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GeneradoNominaSystem.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Empresa> Empresas => Set<Empresa>();

    public DbSet<Empleado> Empleados => Set<Empleado>();

    public DbSet<PeriodoNomina> PeriodosNomina => Set<PeriodoNomina>();

    public DbSet<ConceptoNomina> ConceptosNomina => Set<ConceptoNomina>();

    public DbSet<Nomina> Nominas => Set<Nomina>();

    public DbSet<DetalleNomina> DetallesNomina => Set<DetalleNomina>();

    public DbSet<PlantillaNomina> PlantillasNomina => Set<PlantillaNomina>();

    public DbSet<PlantillaConcepto> PlantillasConcepto => Set<PlantillaConcepto>();

    public DbSet<Cotizacion> Cotizaciones => Set<Cotizacion>();

    public DbSet<DetalleCotizacion> DetallesCotizacion => Set<DetalleCotizacion>();

    public DbSet<ProductoServicio> ProductosServicios => Set<ProductoServicio>();

    public DbSet<PlantillaCotizacion> PlantillasCotizacion => Set<PlantillaCotizacion>();

    public DbSet<Documento> Documentos => Set<Documento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
