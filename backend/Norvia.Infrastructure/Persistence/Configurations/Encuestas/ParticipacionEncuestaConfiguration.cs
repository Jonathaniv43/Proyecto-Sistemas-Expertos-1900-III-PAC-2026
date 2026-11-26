using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Infrastructure.Persistence.Configurations.Encuestas;

public class ParticipacionEncuestaConfiguration : IEntityTypeConfiguration<ParticipacionEncuesta>
{
    public void Configure(EntityTypeBuilder<ParticipacionEncuesta> b)
    {
        b.HasKey(p => new { p.EncuestaId, p.VinculoEmpresaId });
        b.Property(p => p.EmpresaId).IsRequired();
        b.Property(p => p.ParticipoEn).FechaDelServidor();

        b.HasOne(p => p.Empresa).WithMany().HasForeignKey(p => p.EmpresaId);
        b.HasOne(p => p.Encuesta)
            .WithMany()
            .HasForeignKey(p => new { p.EmpresaId, p.EncuestaId })
            .HasPrincipalKey(e => new { e.EmpresaId, e.Id });
        b.HasOne(p => p.VinculoEmpresa)
            .WithMany()
            .HasForeignKey(p => new { p.EmpresaId, p.VinculoEmpresaId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });

        b.HasIndex(p => new { p.EmpresaId, p.VinculoEmpresaId });
        b.HasIndex(p => new { p.EmpresaId, p.EncuestaId });
    }
}
