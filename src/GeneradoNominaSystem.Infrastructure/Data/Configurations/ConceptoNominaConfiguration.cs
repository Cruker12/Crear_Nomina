using GeneradoNominaSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneradoNominaSystem.Infrastructure.Data.Configurations;

public sealed class ConceptoNominaConfiguration : IEntityTypeConfiguration<ConceptoNomina>
{
    public void Configure(EntityTypeBuilder<ConceptoNomina> builder)
    {
        builder.ToTable("ConceptosNomina");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Subtipo).HasMaxLength(150);
        builder.Property(e => e.FormulaCalculo).HasMaxLength(500);

        builder.OwnsOne(e => e.ValorFijo, v =>
        {
            v.Property(x => x.Monto).HasColumnName("ValorFijo_Monto").HasColumnType("decimal(18,2)");
            v.Property(x => x.Moneda).HasColumnName("ValorFijo_Moneda").IsRequired().HasMaxLength(3);
        });
    }
}
