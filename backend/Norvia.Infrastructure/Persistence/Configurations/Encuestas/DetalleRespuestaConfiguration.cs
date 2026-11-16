using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Infrastructure.Persistence.Configurations.Encuestas;

public class DetalleRespuestaConfiguration : IEntityTypeConfiguration<DetalleRespuesta>
{
    public void Configure(EntityTypeBuilder<DetalleRespuesta> b)
    {
        b.HasKey(d => new { d.RespuestaEncuestaId, d.EncuestaPreguntaId });
        b.Property(d => d.EmpresaId).IsRequired();
        b.HasAlternateKey(d => new { d.EmpresaId, d.RespuestaEncuestaId, d.EncuestaPreguntaId });

        b.HasOne(d => d.Empresa).WithMany().HasForeignKey(d => d.EmpresaId);
        b.HasOne(d => d.RespuestaEncuesta)
            .WithMany(r => r.Detalles)
            .HasForeignKey(d => new { d.EmpresaId, d.RespuestaEncuestaId })
            .HasPrincipalKey(r => new { r.EmpresaId, r.Id });
        b.HasOne(d => d.EncuestaPregunta)
            .WithMany()
            .HasForeignKey(d => new { d.EmpresaId, d.EncuestaPreguntaId })
            .HasPrincipalKey(p => new { p.EmpresaId, p.Id });
        b.HasOne(d => d.EncuestaOpcion)
            .WithMany()
            .HasForeignKey(d => new { d.EncuestaPreguntaId, d.EncuestaOpcionId })
            .HasPrincipalKey(o => new { o.EncuestaPreguntaId, o.Id })
            .IsRequired(false);

        b.Check("ValorUnico",
            "(\"EncuestaOpcionId\" IS NULL OR \"TextoLibre\" IS NULL)");

        b.HasIndex(d => new { d.EncuestaPreguntaId, d.EncuestaOpcionId });
    }
}
