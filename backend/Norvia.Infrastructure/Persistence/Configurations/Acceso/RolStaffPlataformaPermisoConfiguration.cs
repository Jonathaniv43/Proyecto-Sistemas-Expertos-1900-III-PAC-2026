using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Acceso;

/// <summary>Solo permisos con Alcance = plataforma (RN-63, RN-64). Lo valida la aplicación.</summary>
public class RolStaffPlataformaPermisoConfiguration : IEntityTypeConfiguration<RolStaffPlataformaPermiso>
{
    public void Configure(EntityTypeBuilder<RolStaffPlataformaPermiso> b)
    {
        b.HasKey(rp => new { rp.RolStaffPlataformaId, rp.PermisoId });
        b.HasOne(rp => rp.RolStaffPlataforma).WithMany(r => r.Permisos).HasForeignKey(rp => rp.RolStaffPlataformaId);
        b.HasOne(rp => rp.Permiso).WithMany(p => p.RolesStaff).HasForeignKey(rp => rp.PermisoId);
    }
}
