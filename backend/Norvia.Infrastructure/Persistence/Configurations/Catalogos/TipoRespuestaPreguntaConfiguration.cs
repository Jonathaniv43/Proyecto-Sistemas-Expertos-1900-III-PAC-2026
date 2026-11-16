using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Infrastructure.Persistence.Configurations.Catalogos;

public class TipoRespuestaPreguntaConfiguration : IEntityTypeConfiguration<TipoRespuestaPreguntaCatalogo>
{
    public void Configure(EntityTypeBuilder<TipoRespuestaPreguntaCatalogo> b)
    {
        b.ToTable("TipoRespuestaPregunta");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Codigo).HasMaxLength(40).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
        b.Property(x => x.InstruccionUi).HasMaxLength(300).IsRequired();
        b.Property(x => x.Activo).PorDefectoTrue();
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}
