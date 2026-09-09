using GeneradoNominaSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneradoNominaSystem.Infrastructure.Data.Configurations;

public sealed class ProductoServicioConfiguration : IEntityTypeConfiguration<ProductoServicio>
{
    public void Configure(EntityTypeBuilder<ProductoServicio> builder)
    {
        builder.ToTable("ProductosServicios");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre).IsRequired().HasMaxLength(250);
        builder.Property(e => e.Descripcion).HasMaxLength(1000);
        builder.Property(e => e.Unidad).IsRequired().HasMaxLength(50);

        builder.HasOne<Empresa>().WithMany().HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(e => e.PrecioUnitario, v =>
        {
            v.Property(x => x.Monto).HasColumnName("PrecioUnitario_Monto").HasColumnType("decimal(18,2)");
            v.Property(x => x.Moneda).HasColumnName("PrecioUnitario_Moneda").IsRequired().HasMaxLength(3);
        });
    }
}
