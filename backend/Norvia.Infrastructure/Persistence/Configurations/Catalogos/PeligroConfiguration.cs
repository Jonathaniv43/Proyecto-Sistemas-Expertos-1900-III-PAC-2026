using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Catalogos;

public class PeligroConfiguration : IEntityTypeConfiguration<Peligro>
{
    public void Configure(EntityTypeBuilder<Peligro> b)
    {
        b.HasKey(p => p.Id);
        b.Property(p => p.Codigo).HasMaxLength(20).IsRequired();
        b.Property(p => p.Nombre).HasMaxLength(150).IsRequired();
        b.Property(p => p.Descripcion);
        b.EnumComoTexto(p => p.Clasificacion, 15);
        b.Property(p => p.Activo).PorDefectoTrue();
        b.HasIndex(p => p.Codigo).IsUnique();
    }
}
