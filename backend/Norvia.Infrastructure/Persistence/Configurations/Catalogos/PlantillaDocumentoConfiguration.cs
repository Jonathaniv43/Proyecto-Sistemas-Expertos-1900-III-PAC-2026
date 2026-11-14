using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Infrastructure.Persistence.Configurations.Catalogos;

public class PlantillaDocumentoConfiguration : IEntityTypeConfiguration<PlantillaDocumento>
{
    public void Configure(EntityTypeBuilder<PlantillaDocumento> b)
    {
        b.HasKey(p => p.Id);
        b.Property(p => p.Codigo).HasMaxLength(30).IsRequired();
        b.Property(p => p.Titulo).HasMaxLength(200).IsRequired();
        b.EnumComoTexto(p => p.TipoDocumento, 15);
        b.Property(p => p.Contenido).IsRequired();
        b.Property(p => p.Activa).PorDefectoTrue();

        b.HasOne(p => p.Rubro).WithMany().HasForeignKey(p => p.RubroId); // nulo = todos los rubros

        b.HasIndex(p => p.Codigo).IsUnique();
    }
}
