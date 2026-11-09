using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Empresas;

/// <summary>Tabla raíz: no lleva EmpresaId porque es la empresa.</summary>
public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.RazonSocial).HasMaxLength(200).IsRequired();
        b.Property(e => e.RTN).HasMaxLength(14).IsFixedLength().IsRequired(); // char(14)
        b.Check("RTN", "\"RTN\" ~ '^[0-9]{14}$'"); // RN-01: 14 dígitos
        b.Property(e => e.Direccion).HasMaxLength(300);
        b.Property(e => e.ActividadEconomica).HasMaxLength(150);
        b.Property(e => e.TrabajadoresPermanentes);
        b.Check("TrabajadoresPermanentes", "\"TrabajadoresPermanentes\" >= 0");
        b.EnumComoTexto(e => e.Estado, 12);
        b.Property(e => e.CreadaEn).FechaDelServidor();

        // RubroId es nulo hasta completar el perfil (RN-09).
        b.HasOne(e => e.Rubro).WithMany().HasForeignKey(e => e.RubroId);

        b.HasIndex(e => e.RTN).IsUnique();
    }
}
