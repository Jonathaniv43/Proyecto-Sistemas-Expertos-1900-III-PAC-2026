using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Acceso;

public class StaffPlataformaConfiguration : IEntityTypeConfiguration<StaffPlataforma>
{
    public void Configure(EntityTypeBuilder<StaffPlataforma> b)
    {
        b.HasKey(s => s.Id);
        b.Property(s => s.Email).HasMaxLength(254).IsRequired();
        b.Property(s => s.PasswordHash).HasMaxLength(255).IsRequired();
        b.Property(s => s.DebeCambiarPassword).PorDefectoFalse();
        b.Property(s => s.Activo).PorDefectoTrue();
        b.Property(s => s.CreadoEn).FechaDelServidor();

        b.HasOne(s => s.RolStaffPlataforma)
            .WithMany(r => r.Staff)
            .HasForeignKey(s => s.RolStaffPlataformaId);

        // Correo único sin distinguir mayúsculas: se guarda siempre en minúsculas
        // (lo normaliza la aplicación) y la base lo exige con el CHECK.
        b.HasIndex(s => s.Email).IsUnique();
        b.Check("EmailEnMinusculas", "\"Email\" = lower(\"Email\")");
    }
}
