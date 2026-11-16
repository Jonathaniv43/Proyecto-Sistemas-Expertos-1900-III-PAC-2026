using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Infrastructure.Persistence.Configurations.Encuestas;

public class EncuestaPreguntaConfiguration : IEntityTypeConfiguration<EncuestaPregunta>
{
    public void Configure(EntityTypeBuilder<EncuestaPregunta> b)
    {
        b.HasKey(p => p.Id);
        b.TablaDeEmpresa();

        b.Property(p => p.Texto).HasMaxLength(500).IsRequired();
        b.EnumComoTexto(p => p.TipoRespuesta, 25);
        b.Property(p => p.Obligatoria).PorDefectoTrue();

        b.HasOne(p => p.Empresa).WithMany().HasForeignKey(p => p.EmpresaId);
        b.HasOne(p => p.EncuestaVersion)
            .WithMany(v => v.Preguntas)
            .HasForeignKey(p => new { p.EmpresaId, p.EncuestaVersionId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });
        b.HasOne(p => p.PreguntaExploratoria).WithMany().HasForeignKey(p => p.PreguntaExploratoriaId);
        b.HasOne(p => p.Peligro).WithMany().HasForeignKey(p => p.PeligroId);

        b.HasIndex(p => new { p.EncuestaVersionId, p.Orden }).IsUnique();
        b.HasAlternateKey(p => new { p.EncuestaVersionId, p.Id });

        b.Check("RespuestaBreveSinPeligro", "\"TipoRespuesta\" <> 'respuesta_breve' OR \"PeligroId\" IS NULL");
    }
}
