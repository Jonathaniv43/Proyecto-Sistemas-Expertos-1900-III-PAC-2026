using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Infrastructure.Persistence.Configurations.Encuestas;

public class DetalleRespuestaOpcionConfiguration : IEntityTypeConfiguration<DetalleRespuestaOpcion>
{
    public void Configure(EntityTypeBuilder<DetalleRespuestaOpcion> b)
    {
        b.HasKey(o => new { o.RespuestaEncuestaId, o.EncuestaPreguntaId, o.EncuestaOpcionId });
        b.Property(o => o.EmpresaId).IsRequired();

        b.HasOne(o => o.Empresa).WithMany().HasForeignKey(o => o.EmpresaId);
        b.HasOne(o => o.DetalleRespuesta)
            .WithMany(d => d.Opciones)
            .HasForeignKey(o => new { o.EmpresaId, o.RespuestaEncuestaId, o.EncuestaPreguntaId })
            .HasPrincipalKey(d => new { d.EmpresaId, d.RespuestaEncuestaId, d.EncuestaPreguntaId });
        b.HasOne(o => o.EncuestaOpcion)
            .WithMany()
            .HasForeignKey(o => new { o.EncuestaPreguntaId, o.EncuestaOpcionId })
            .HasPrincipalKey(op => new { op.EncuestaPreguntaId, op.Id });

        b.HasIndex(o => new { o.EncuestaPreguntaId, o.EncuestaOpcionId });
    }
}
