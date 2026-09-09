using GeneradoNominaSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneradoNominaSystem.Infrastructure.Data.Configurations;

public sealed class CotizacionConfiguration : IEntityTypeConfiguration<Cotizacion>
{
    public void Configure(EntityTypeBuilder<Cotizacion> builder)
    {
        builder.ToTable("Cotizaciones");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ClienteNombre).IsRequired().HasMaxLength(250);
        builder.Property(e => e.ClienteDocumento).HasMaxLength(50);
        builder.Property(e => e.ClienteEmail).HasMaxLength(200);
        builder.Property(e => e.ClienteTelefono).HasMaxLength(50);
        builder.Property(e => e.NumeroCotizacion).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Observaciones).HasMaxLength(2000);
        builder.Property(e => e.Moneda).IsRequired().HasMaxLength(3);

        builder.HasIndex(e => new { e.EmpresaId, e.NumeroCotizacion }).IsUnique();

        builder.HasOne<Empresa>().WithMany().HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PlantillaCotizacion>().WithMany().HasForeignKey(e => e.PlantillaCotizacionId).OnDelete(DeleteBehavior.SetNull);

        builder.OwnsOne(e => e.Subtotal, v =>
        {
            v.Property(x => x.Monto).HasColumnName("Subtotal_Monto").HasColumnType("decimal(18,2)");
            v.Property(x => x.Moneda).HasColumnName("Subtotal_Moneda").IsRequired().HasMaxLength(3);
        });

        builder.OwnsOne(e => e.TotalDescuentos, v =>
        {
            v.Property(x => x.Monto).HasColumnName("TotalDescuentos_Monto").HasColumnType("decimal(18,2)");
            v.Property(x => x.Moneda).HasColumnName("TotalDescuentos_Moneda").IsRequired().HasMaxLength(3);
        });

        builder.OwnsOne(e => e.TotalNeto, v =>
        {
            v.Property(x => x.Monto).HasColumnName("TotalNeto_Monto").HasColumnType("decimal(18,2)");
            v.Property(x => x.Moneda).HasColumnName("TotalNeto_Moneda").IsRequired().HasMaxLength(3);
        });
    }
}
