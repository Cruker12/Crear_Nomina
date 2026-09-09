using GeneradoNominaSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneradoNominaSystem.Infrastructure.Data.Configurations;

public sealed class NominaConfiguration : IEntityTypeConfiguration<Nomina>
{
    public void Configure(EntityTypeBuilder<Nomina> builder)
    {
        builder.ToTable("Nominas");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Observaciones).HasMaxLength(1000);

        builder.HasOne<Empresa>().WithMany().HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Empleado>().WithMany().HasForeignKey(e => e.EmpleadoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PeriodoNomina>().WithMany().HasForeignKey(e => e.PeriodoNominaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PlantillaNomina>().WithMany().HasForeignKey(e => e.PlantillaNominaId).OnDelete(DeleteBehavior.SetNull);

        builder.OwnsOne(e => e.SubtotalDevengos, v =>
        {
            v.Property(x => x.Monto).HasColumnName("SubtotalDevengos_Monto").HasColumnType("decimal(18,2)");
            v.Property(x => x.Moneda).HasColumnName("SubtotalDevengos_Moneda").IsRequired().HasMaxLength(3);
        });

        builder.OwnsOne(e => e.SubtotalDeducciones, v =>
        {
            v.Property(x => x.Monto).HasColumnName("SubtotalDeducciones_Monto").HasColumnType("decimal(18,2)");
            v.Property(x => x.Moneda).HasColumnName("SubtotalDeducciones_Moneda").IsRequired().HasMaxLength(3);
        });

        builder.OwnsOne(e => e.TotalNeto, v =>
        {
            v.Property(x => x.Monto).HasColumnName("TotalNeto_Monto").HasColumnType("decimal(18,2)");
            v.Property(x => x.Moneda).HasColumnName("TotalNeto_Moneda").IsRequired().HasMaxLength(3);
        });
    }
}
