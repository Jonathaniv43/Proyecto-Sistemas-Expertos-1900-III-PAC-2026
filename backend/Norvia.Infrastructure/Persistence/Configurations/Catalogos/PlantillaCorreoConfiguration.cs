using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Infrastructure.Persistence.Configurations.Catalogos;

public class PlantillaCorreoConfiguration : IEntityTypeConfiguration<PlantillaCorreo>
{
    public void Configure(EntityTypeBuilder<PlantillaCorreo> b)
    {
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedNever();
        b.Property(p => p.Codigo).HasMaxLength(40).IsRequired();
        b.Property(p => p.Nombre).HasMaxLength(80).IsRequired();
        b.Property(p => p.Descripcion).HasMaxLength(300).IsRequired();
        b.Property(p => p.Asunto).HasMaxLength(200).IsRequired();
        b.Property(p => p.CuerpoHtml).IsRequired();
        b.Property(p => p.Activo).PorDefectoTrue();
        b.HasIndex(p => p.Codigo).IsUnique();
    }
}