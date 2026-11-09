using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Acceso;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> b)
    {
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).ValueGeneratedNever(); // IDs fijos, los pone el seed
        b.Property(r => r.Codigo).HasMaxLength(30).IsRequired();
        b.Property(r => r.Nombre).HasMaxLength(60).IsRequired();
        b.HasIndex(r => r.Codigo).IsUnique();
    }
}
