using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Acceso;

public class VinculoEmpresaConfiguration : IEntityTypeConfiguration<VinculoEmpresa>
{
    public void Configure(EntityTypeBuilder<VinculoEmpresa> b)
    {
        b.HasKey(v => v.Id);
        b.TablaDeEmpresa(); // UQ (EmpresaId, Id): destino de ResponsableId, EncargadoId, etc.

        b.Property(v => v.Email).HasMaxLength(254).IsRequired();
        b.Property(v => v.PasswordHash).HasMaxLength(255).IsRequired();
        b.Property(v => v.DebeCambiarPassword).PorDefectoFalse();
        b.EnumComoTexto(v => v.Estado, 12);
        b.Property(v => v.CreadoEn).FechaDelServidor();

        b.HasOne(v => v.Empresa).WithMany(e => e.Vinculos).HasForeignKey(v => v.EmpresaId);
        b.HasOne(v => v.Persona).WithMany(p => p.Vinculos).HasForeignKey(v => v.PersonaId); // Persona es global: FK simple
        b.HasOne(v => v.Rol).WithMany(r => r.Vinculos).HasForeignKey(v => v.RolId);

        // Una persona, un vínculo por empresa.
        b.HasIndex(v => new { v.EmpresaId, v.PersonaId }).IsUnique();

        // Correo institucional único sin distinguir mayúsculas (RN-57): se guarda siempre en minúsculas
        // (lo normaliza la aplicación) y la base lo exige con el CHECK.
        b.HasIndex(v => v.Email).IsUnique();
        b.Check("EmailEnMinusculas", "\"Email\" = lower(\"Email\")");
    }
}
