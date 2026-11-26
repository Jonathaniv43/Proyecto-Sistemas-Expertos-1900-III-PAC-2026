using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Infrastructure.Persistence.Configurations.Encuestas;

public class EncuestaOpcionConfiguration : IEntityTypeConfiguration<EncuestaOpcion>
{
    public void Configure(EntityTypeBuilder<EncuestaOpcion> b)
    {
        b.HasKey(o => o.Id);
        b.TablaDeEmpresa();

        b.Property(o => o.Texto).HasMaxLength(200).IsRequired();
        b.Property(o => o.IndicaExposicion).PorDefectoFalse();

        b.HasOne(o => o.Empresa).WithMany().HasForeignKey(o => o.EmpresaId);
        b.HasOne(o => o.EncuestaPregunta)
            .WithMany(p => p.Opciones)
            .HasForeignKey(o => new { o.EmpresaId, o.EncuestaPreguntaId })
            .HasPrincipalKey(p => new { p.EmpresaId, p.Id });

        b.HasIndex(o => new { o.EncuestaPreguntaId, o.Orden }).IsUnique();
        b.HasAlternateKey(o => new { o.EncuestaPreguntaId, o.Id });
    }
}
