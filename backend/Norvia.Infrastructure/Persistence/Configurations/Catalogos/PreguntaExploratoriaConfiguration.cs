using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Catalogos;

public class PreguntaExploratoriaConfiguration : IEntityTypeConfiguration<PreguntaExploratoria>
{
    public void Configure(EntityTypeBuilder<PreguntaExploratoria> b)
    {
        b.HasKey(p => p.Id);
        b.Property(p => p.Texto).HasMaxLength(500).IsRequired();
        b.EnumComoTexto(p => p.TipoRespuesta, 25);
        b.Property(p => p.Obligatoria).PorDefectoTrue();
        b.Property(p => p.Activa).PorDefectoTrue();

        b.HasOne(p => p.CategoriaRiesgo).WithMany(c => c.Preguntas).HasForeignKey(p => p.CategoriaRiesgoId);
        b.HasOne(p => p.Peligro).WithMany().HasForeignKey(p => p.PeligroId);

        b.HasIndex(p => new { p.CategoriaRiesgoId, p.Orden }).IsUnique();

        // Una respuesta libre no puede indicar exposición.
        b.Check("RespuestaBreveSinPeligro", "\"TipoRespuesta\" <> 'respuesta_breve' OR \"PeligroId\" IS NULL");
    }
}
