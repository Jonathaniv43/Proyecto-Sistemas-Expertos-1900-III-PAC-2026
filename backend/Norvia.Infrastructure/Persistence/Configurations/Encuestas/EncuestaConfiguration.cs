using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Infrastructure.Persistence.Configurations.Encuestas;

public class EncuestaConfiguration : IEntityTypeConfiguration<Encuesta>
{
    public void Configure(EntityTypeBuilder<Encuesta> b)
    {
        b.HasKey(e => e.Id);
        b.TablaDeEmpresa();

        b.Property(e => e.Titulo).HasMaxLength(200).IsRequired();
        b.EnumComoTexto(e => e.Estado, 10);
        b.Property(e => e.CreadaEn).FechaDelServidor();

        b.HasOne(e => e.Empresa).WithMany().HasForeignKey(e => e.EmpresaId);
        b.HasOne(e => e.CreadaPor)
            .WithMany()
            .HasForeignKey(e => new { e.EmpresaId, e.CreadaPorId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });

        b.Check("CerradaEn", "(\"CerradaEn\" IS NOT NULL) = (\"Estado\" = 'cerrada')");
        b.HasIndex(e => new { e.EmpresaId, e.Estado });
    }
}
