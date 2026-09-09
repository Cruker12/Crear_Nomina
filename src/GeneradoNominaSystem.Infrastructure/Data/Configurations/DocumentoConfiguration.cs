using GeneradoNominaSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneradoNominaSystem.Infrastructure.Data.Configurations;

public sealed class DocumentoConfiguration : IEntityTypeConfiguration<Documento>
{
    public void Configure(EntityTypeBuilder<Documento> builder)
    {
        builder.ToTable("Documentos");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.NumeroDocumento).IsRequired().HasMaxLength(100);
        builder.Property(e => e.RutaArchivo).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.GeneradoPor).HasMaxLength(200);

        builder.HasOne<Empresa>().WithMany().HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict);
    }
}
