using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Infrastructure.Persistence.Configurations.Encuestas;

public class EncuestaSeccionConfiguration : IEntityTypeConfiguration<EncuestaSeccion>
{
    public void Configure(EntityTypeBuilder<EncuestaSeccion> b)
    {
        b.HasKey(s => s.Id);
        b.TablaDeEmpresa();

        b.Property(s => s.Titulo).HasMaxLength(200).IsRequired();

        b.HasOne(s => s.Empresa).WithMany().HasForeignKey(s => s.EmpresaId);
        b.HasOne(s => s.EncuestaVersion)
            .WithMany(v => v.Secciones)
            .HasForeignKey(s => new { s.EmpresaId, s.EncuestaVersionId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });

        b.HasIndex(s => new { s.EncuestaVersionId, s.Orden }).IsUnique();
        b.HasAlternateKey(s => new { s.EncuestaVersionId, s.Id });
    }
}
