using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Infrastructure.Persistence.Configurations.Encuestas;

public class EncuestaVersionConfiguration : IEntityTypeConfiguration<EncuestaVersion>
{
    public void Configure(EntityTypeBuilder<EncuestaVersion> b)
    {
        b.HasKey(v => v.Id);
        b.TablaDeEmpresa();

        b.EnumComoTexto(v => v.Estado, 12);
        b.Property(v => v.CreadaEn).FechaDelServidor();

        b.HasOne(v => v.Empresa).WithMany().HasForeignKey(v => v.EmpresaId);
        b.HasOne(v => v.Encuesta)
            .WithMany(e => e.Versiones)
            .HasForeignKey(v => new { v.EmpresaId, v.EncuestaId })
            .HasPrincipalKey(e => new { e.EmpresaId, e.Id });
        b.HasOne(v => v.CreadaPor)
            .WithMany()
            .HasForeignKey(v => new { v.EmpresaId, v.CreadaPorId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });

        b.HasIndex(v => new { v.EncuestaId, v.NumeroVersion }).IsUnique();

        // Solo una versión publicada y un borrador a la vez por encuesta (D8, RN-12, RN-70).
        b.HasIndex([nameof(EncuestaVersion.EncuestaId)], "IX_EncuestaVersion_EncuestaId_Publicada")
            .IsUnique()
            .HasFilter("\"Estado\" = 'publicada'");
        b.HasIndex([nameof(EncuestaVersion.EncuestaId)], "IX_EncuestaVersion_EncuestaId_Borrador")
            .IsUnique()
            .HasFilter("\"Estado\" = 'borrador'");

        b.Check("NumeroVersion", "\"NumeroVersion\" >= 1");
        b.Check("PublicadaEn", "\"Estado\" = 'borrador' OR \"PublicadaEn\" IS NOT NULL");
    }
}
