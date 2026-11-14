using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Catalogos;

public class PlantillaTareaConfiguration : IEntityTypeConfiguration<PlantillaTarea>
{
    public void Configure(EntityTypeBuilder<PlantillaTarea> b)
    {
        b.HasKey(p => p.Id);
        b.EnumComoTexto<PlantillaTarea, TipoControl>(p => p.TipoControl, 15); // mismos valores que Control.Tipo
        b.Property(p => p.Titulo).HasMaxLength(200).IsRequired();
        b.Property(p => p.Descripcion);
        b.Property(p => p.RequiereEvidencia).PorDefectoFalse();
        b.Property(p => p.Activa).PorDefectoTrue();

        b.HasOne(p => p.Peligro).WithMany().HasForeignKey(p => p.PeligroId);

        b.Check("PrioridadSugerida", "\"PrioridadSugerida\" BETWEEN 1 AND 3");
    }
}
