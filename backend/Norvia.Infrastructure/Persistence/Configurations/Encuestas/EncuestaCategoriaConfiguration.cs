using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Infrastructure.Persistence.Configurations.Encuestas;

public class EncuestaCategoriaConfiguration : IEntityTypeConfiguration<EncuestaCategoria>
{
    public void Configure(EntityTypeBuilder<EncuestaCategoria> b)
    {
        b.HasKey(ec => new { ec.EncuestaVersionId, ec.CategoriaRiesgoId });
        b.Property(ec => ec.EmpresaId).IsRequired();

        b.HasOne(ec => ec.Empresa).WithMany().HasForeignKey(ec => ec.EmpresaId);
        b.HasOne(ec => ec.EncuestaVersion)
            .WithMany(v => v.Categorias)
            .HasForeignKey(ec => new { ec.EmpresaId, ec.EncuestaVersionId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });
        b.HasOne(ec => ec.CategoriaRiesgo)
            .WithMany()
            .HasForeignKey(ec => ec.CategoriaRiesgoId);

        b.HasIndex(ec => new { ec.EmpresaId, ec.EncuestaVersionId });
    }
}
