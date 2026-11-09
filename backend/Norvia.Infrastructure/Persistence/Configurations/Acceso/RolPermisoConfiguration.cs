using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Acceso;

/// <summary>
/// Solo permisos con Alcance = empresa (RN-63). Esa regla cruza tablas, así que no cabe
/// en un CHECK: la valida la aplicación al asignar permisos.
/// </summary>
public class RolPermisoConfiguration : IEntityTypeConfiguration<RolPermiso>
{
    public void Configure(EntityTypeBuilder<RolPermiso> b)
    {
        b.HasKey(rp => new { rp.RolId, rp.PermisoId });
        b.HasOne(rp => rp.Rol).WithMany(r => r.Permisos).HasForeignKey(rp => rp.RolId);
        b.HasOne(rp => rp.Permiso).WithMany(p => p.Roles).HasForeignKey(rp => rp.PermisoId);
    }
}
