using GeneradoNominaSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneradoNominaSystem.Infrastructure.Data.Configurations;

public sealed class PlantillaConceptoConfiguration : IEntityTypeConfiguration<PlantillaConcepto>
{
    public void Configure(EntityTypeBuilder<PlantillaConcepto> builder)
    {
        builder.ToTable("PlantillasConcepto");
        builder.HasKey(e => e.Id);

        builder.HasOne<PlantillaNomina>().WithMany(p => p.Conceptos).HasForeignKey(e => e.PlantillaNominaId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ConceptoNomina>().WithMany().HasForeignKey(e => e.ConceptoNominaId).OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(e => e.ValorPorDefecto, v =>
        {
            v.Property(x => x.Monto).HasColumnName("ValorPorDefecto_Monto").HasColumnType("decimal(18,2)");
            v.Property(x => x.Moneda).HasColumnName("ValorPorDefecto_Moneda").IsRequired().HasMaxLength(3);
        });
    }
}
