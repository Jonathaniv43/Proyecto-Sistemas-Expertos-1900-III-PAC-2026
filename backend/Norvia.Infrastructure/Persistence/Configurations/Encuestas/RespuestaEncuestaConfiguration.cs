using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Infrastructure.Persistence.Configurations.Encuestas;

public class RespuestaEncuestaConfiguration : IEntityTypeConfiguration<RespuestaEncuesta>
{
    public void Configure(EntityTypeBuilder<RespuestaEncuesta> b)
    {
        b.HasKey(r => r.Id);
        b.TablaDeEmpresa();

        b.Property(r => r.FechaRespuesta).HasColumnType("date").IsRequired();

        b.HasOne(r => r.Empresa).WithMany().HasForeignKey(r => r.EmpresaId);
        b.HasOne(r => r.EncuestaVersion)
            .WithMany()
            .HasForeignKey(r => new { r.EmpresaId, r.EncuestaVersionId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });

        b.HasAlternateKey(r => new { r.EncuestaVersionId, r.Id });
        b.HasIndex(r => new { r.EmpresaId, r.EncuestaVersionId });
    }
}
