using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EntidadAcceso = Norvia.Domain.Entities.Acceso.Acceso;

namespace Norvia.Infrastructure.Persistence.Configurations.Acceso;

public class AccesoConfiguration : IEntityTypeConfiguration<EntidadAcceso>
{
    public void Configure(EntityTypeBuilder<EntidadAcceso> b)
    {
        b.HasKey(a => a.Id);
        b.Property(a => a.Email).HasMaxLength(254).IsRequired();
        b.Property(a => a.PasswordHash).HasMaxLength(255).IsRequired();
        b.Property(a => a.DebeCambiarPassword).PorDefectoFalse();
        b.Property(a => a.Verificado).PorDefectoFalse();
        b.Property(a => a.CreadoEn).FechaDelServidor();

        b.HasIndex(a => a.Email).IsUnique();
        b.Check("EmailEnMinusculas", "\"Email\" = lower(\"Email\")");
    }
}
