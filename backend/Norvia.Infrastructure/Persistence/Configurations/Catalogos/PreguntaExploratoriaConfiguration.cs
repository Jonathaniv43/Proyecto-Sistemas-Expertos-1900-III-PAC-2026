using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Infrastructure.Persistence.Configurations.Catalogos;

public class PreguntaExploratoriaConfiguration : IEntityTypeConfiguration<PreguntaExploratoria>
{
    public void Configure(EntityTypeBuilder<PreguntaExploratoria> b)
    {
        b.HasKey(p => p.Id);
        b.Property(p => p.Texto).HasMaxLength(500).IsRequired();
        b.Property(p => p.Obligatoria).PorDefectoTrue();
        b.Property(p => p.Activa).PorDefectoTrue();

        b.HasOne(p => p.CategoriaRiesgo).WithMany(c => c.Preguntas).HasForeignKey(p => p.CategoriaRiesgoId);
        b.HasOne(p => p.TipoRespuestaPregunta).WithMany().HasForeignKey(p => p.TipoRespuestaPreguntaId);
        b.HasOne(p => p.Peligro).WithMany().HasForeignKey(p => p.PeligroId);

        b.HasIndex(p => new { p.CategoriaRiesgoId, p.Orden }).IsUnique();
        // Respuesta breve sin peligro: validación en Application (el Id del tipo es runtime vía seeder).
    }
}
