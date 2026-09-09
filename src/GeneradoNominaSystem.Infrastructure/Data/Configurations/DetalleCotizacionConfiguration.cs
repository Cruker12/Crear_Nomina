using GeneradoNominaSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneradoNominaSystem.Infrastructure.Data.Configurations;

public sealed class DetalleCotizacionConfiguration : IEntityTypeConfiguration<DetalleCotizacion>
{
    public void Configure(EntityTypeBuilder<DetalleCotizacion> builder)
    {
        builder.ToTable("DetallesCotizacion");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Descripcion).IsRequired().HasMaxLength(500);

        builder.HasOne<Cotizacion>().WithMany(c => c.Detalles).HasForeignKey(e => e.CotizacionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ProductoServicio>().WithMany().HasForeignKey(e => e.ProductoServicioId).OnDelete(DeleteBehavior.SetNull);

        builder.OwnsOne(e => e.PrecioUnitario, v =>
        {
            v.Property(x => x.Monto).HasColumnName("PrecioUnitario_Monto").HasColumnType("decimal(18,2)");
            v.Property(x => x.Moneda).HasColumnName("PrecioUnitario_Moneda").IsRequired().HasMaxLength(3);
        });

        builder.OwnsOne(e => e.Subtotal, v =>
        {
            v.Property(x => x.Monto).HasColumnName("Subtotal_Monto").HasColumnType("decimal(18,2)");
            v.Property(x => x.Moneda).HasColumnName("Subtotal_Moneda").IsRequired().HasMaxLength(3);
        });
    }
}
