using GeneradoNominaSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneradoNominaSystem.Infrastructure.Data.Configurations;

public sealed class DetalleNominaConfiguration : IEntityTypeConfiguration<DetalleNomina>
{
    public void Configure(EntityTypeBuilder<DetalleNomina> builder)
    {
        builder.ToTable("DetallesNomina");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Descripcion).HasMaxLength(500);

        builder.HasOne<Nomina>().WithMany(n => n.Detalles).HasForeignKey(e => e.NominaId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ConceptoNomina>().WithMany().HasForeignKey(e => e.ConceptoNominaId).OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(e => e.Valor, v =>
        {
            v.Property(x => x.Monto).HasColumnName("Valor_Monto").HasColumnType("decimal(18,2)");
            v.Property(x => x.Moneda).HasColumnName("Valor_Moneda").IsRequired().HasMaxLength(3);
        });
    }
}
