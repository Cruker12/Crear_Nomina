using GeneradoNominaSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneradoNominaSystem.Infrastructure.Data.Configurations;

public sealed class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("Empresas");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.RazonSocial).IsRequired().HasMaxLength(250);
        builder.Property(e => e.NombreComercial).IsRequired().HasMaxLength(250);
        builder.Property(e => e.Nit).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Telefono).HasMaxLength(50);
        builder.Property(e => e.Email).HasMaxLength(200);
        builder.Property(e => e.LogoRuta).HasMaxLength(500);

        builder.HasIndex(e => e.Nit).IsUnique();

        builder.OwnsOne(e => e.Direccion, d =>
        {
            d.Property(x => x.DireccionCompleta).HasColumnName("Direccion").IsRequired().HasMaxLength(300);
            d.Property(x => x.Ciudad).HasColumnName("Ciudad").IsRequired().HasMaxLength(150);
            d.Property(x => x.Departamento).HasColumnName("Departamento").IsRequired().HasMaxLength(150);
            d.Property(x => x.Pais).HasColumnName("Pais").IsRequired().HasMaxLength(100);
            d.Property(x => x.CodigoPostal).HasColumnName("CodigoPostal").HasMaxLength(20);
        });

        builder.OwnsOne(e => e.DatosFiscales, f =>
        {
            f.Property(x => x.Nit).HasColumnName("NitFiscal").IsRequired().HasMaxLength(50);
            f.Property(x => x.RazonSocial).HasColumnName("RazonSocialFiscal").IsRequired().HasMaxLength(250);
            f.Property(x => x.DigitoVerificacion).HasColumnName("DigitoVerificacion").HasMaxLength(5);
            f.Property(x => x.RegimenTributario).HasColumnName("RegimenTributario").HasMaxLength(150);
            f.Property(x => x.ResponsableIva).HasColumnName("ResponsableIva");
            f.Property(x => x.RepresentanteLegal).HasColumnName("RepresentanteLegal").HasMaxLength(250);
        });
    }
}
