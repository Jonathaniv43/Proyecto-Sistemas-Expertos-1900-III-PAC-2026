using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Catalogos;

public class CategoriaRiesgoConfiguration : IEntityTypeConfiguration<CategoriaRiesgo>
{
    public void Configure(EntityTypeBuilder<CategoriaRiesgo> b)
    {
        b.HasKey(c => c.Id);
        b.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
        b.Property(c => c.Descripcion);
        b.Property(c => c.Activa).PorDefectoTrue();

        b.HasOne(c => c.Rubro).WithMany(r => r.Categorias).HasForeignKey(c => c.RubroId);

        b.HasIndex(c => new { c.RubroId, c.Nombre }).IsUnique();
    }
}
