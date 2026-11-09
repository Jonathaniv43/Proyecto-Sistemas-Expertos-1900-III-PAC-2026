using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Empresas;

public class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> b)
    {
        b.HasKey(a => a.Id);
        b.TablaDeEmpresa();

        b.Property(a => a.Nombre).HasMaxLength(100).IsRequired();
        b.Property(a => a.Descripcion);
        b.Property(a => a.Activa).PorDefectoTrue();

        b.HasOne(a => a.Empresa).WithMany(e => e.Areas).HasForeignKey(a => a.EmpresaId);

        // FK compuesta: el encargado debe tener vínculo en la misma empresa (D15).
        b.HasOne(a => a.Encargado)
            .WithMany()
            .HasForeignKey(a => new { a.EmpresaId, a.EncargadoId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });

        b.HasIndex(a => new { a.EmpresaId, a.Nombre }).IsUnique();
    }
}
