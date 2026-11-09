using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Catalogos;

/// <summary>Que una pregunta si_no tenga exactamente dos opciones lo valida la aplicación.</summary>
public class OpcionPreguntaConfiguration : IEntityTypeConfiguration<OpcionPregunta>
{
    public void Configure(EntityTypeBuilder<OpcionPregunta> b)
    {
        b.HasKey(o => o.Id);
        b.Property(o => o.Texto).HasMaxLength(200).IsRequired();
        b.Property(o => o.IndicaExposicion).PorDefectoFalse();

        b.HasOne(o => o.PreguntaExploratoria).WithMany(p => p.Opciones).HasForeignKey(o => o.PreguntaExploratoriaId);

        b.HasIndex(o => new { o.PreguntaExploratoriaId, o.Orden }).IsUnique();
    }
}
