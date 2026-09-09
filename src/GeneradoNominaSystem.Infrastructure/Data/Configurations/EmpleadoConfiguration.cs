using GeneradoNominaSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeneradoNominaSystem.Infrastructure.Data.Configurations;

public sealed class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> builder)
    {
        builder.ToTable("Empleados");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.NumeroDocumento).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Nombres).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Apellidos).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Email).HasMaxLength(200);
        builder.Property(e => e.Telefono).HasMaxLength(50);
        builder.Property(e => e.Cargo).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Departamento).HasMaxLength(200);

        builder.HasIndex(e => new { e.EmpresaId, e.NumeroDocumento }).IsUnique();

        builder.HasOne<Empresa>().WithMany().HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(e => e.SalarioBase, s =>
        {
            s.Property(x => x.Monto).HasColumnName("SalarioBase_Monto").HasColumnType("decimal(18,2)");
            s.Property(x => x.Moneda).HasColumnName("SalarioBase_Moneda").IsRequired().HasMaxLength(3);
        });

        builder.OwnsOne(e => e.Direccion, d =>
        {
            d.Property(x => x.DireccionCompleta).HasColumnName("Direccion").IsRequired().HasMaxLength(300);
            d.Property(x => x.Ciudad).HasColumnName("Ciudad").IsRequired().HasMaxLength(150);
            d.Property(x => x.Departamento).HasColumnName("Departamento").IsRequired().HasMaxLength(150);
            d.Property(x => x.Pais).HasColumnName("Pais").IsRequired().HasMaxLength(100);
            d.Property(x => x.CodigoPostal).HasColumnName("CodigoPostal").HasMaxLength(20);
        });
    }
}
