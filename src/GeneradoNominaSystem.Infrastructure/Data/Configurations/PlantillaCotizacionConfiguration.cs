using GeneradoNominaSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneradoNominaSystem.Infrastructure.Data.Configurations;

public sealed class PlantillaCotizacionConfiguration : IEntityTypeConfiguration<PlantillaCotizacion>
{
    public void Configure(EntityTypeBuilder<PlantillaCotizacion> builder)
    {
        builder.ToTable("PlantillasCotizacion");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre).IsRequired().HasMaxLength(200);

        builder.HasOne<Empresa>().WithMany().HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(e => e.ConfiguracionDocumento, c =>
        {
            c.Property(x => x.MostrarLogo).HasColumnName("Doc_MostrarLogo");
            c.Property(x => x.LogoPosicion).HasColumnName("Doc_LogoPosicion");
            c.Property(x => x.EncabezadoPersonalizado).HasColumnName("Doc_Encabezado").HasMaxLength(2000);
            c.Property(x => x.PiePaginaPersonalizado).HasColumnName("Doc_PiePagina").HasMaxLength(2000);
            c.Property(x => x.MostrarFirma).HasColumnName("Doc_MostrarFirma");
            c.Property(x => x.TextoFirma).HasColumnName("Doc_TextoFirma").HasMaxLength(500);
            c.Property(x => x.NumeracionAutomatica).HasColumnName("Doc_NumeracionAutomatica");
            c.Property(x => x.PrefijoNumeracion).HasColumnName("Doc_Prefijo").HasMaxLength(20);
            c.Property(x => x.SiguienteNumero).HasColumnName("Doc_SiguienteNumero");
            c.Property(x => x.FormatoFecha).HasColumnName("Doc_FormatoFecha").IsRequired().HasMaxLength(30);
            c.Property(x => x.SimboloMoneda).HasColumnName("Doc_SimboloMoneda").IsRequired().HasMaxLength(10);
            c.Property(x => x.DecimalesMoneda).HasColumnName("Doc_DecimalesMoneda");
        });
    }
}
