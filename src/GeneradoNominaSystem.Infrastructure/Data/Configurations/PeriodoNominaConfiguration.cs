using GeneradoNominaSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneradoNominaSystem.Infrastructure.Data.Configurations;

public sealed class PeriodoNominaConfiguration : IEntityTypeConfiguration<PeriodoNomina>
{
    public void Configure(EntityTypeBuilder<PeriodoNomina> builder)
    {
        builder.ToTable("PeriodosNomina");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre).IsRequired().HasMaxLength(200);

        builder.HasOne<Empresa>().WithMany().HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict);
    }
}
