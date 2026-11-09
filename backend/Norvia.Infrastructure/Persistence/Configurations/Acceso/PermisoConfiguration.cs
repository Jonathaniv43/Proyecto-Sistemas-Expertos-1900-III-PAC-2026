using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Acceso;

public class PermisoConfiguration : IEntityTypeConfiguration<Permiso>
{
    public void Configure(EntityTypeBuilder<Permiso> b)
    {
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedNever(); // IDs fijos, los pone el seed
        b.Property(p => p.Codigo).HasMaxLength(60).IsRequired();
        b.Property(p => p.Nombre).HasMaxLength(120).IsRequired();
        b.EnumComoTexto(p => p.Alcance, 10); // D24: solo 'empresa' o 'plataforma'
        b.Property(p => p.MetodoHttp).HasMaxLength(10).IsRequired();
        b.Check("MetodoHttp", "\"MetodoHttp\" IN ('GET', 'POST', 'PUT', 'PATCH', 'DELETE')");
        b.Property(p => p.RutaPatron).HasMaxLength(200).IsRequired();
        b.Property(p => p.Activo).PorDefectoTrue();
        b.HasIndex(p => p.Codigo).IsUnique();
    }
}
