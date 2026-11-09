using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Acceso;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.HasKey(u => u.Id);
        b.Property(u => u.Activo).PorDefectoTrue();
        b.Property(u => u.CreadoEn).FechaDelServidor();

        // 1:1 con Persona: una fila por persona que usa la app (UQ en PersonaId).
        b.HasOne(u => u.Persona)
            .WithOne(p => p.Usuario)
            .HasForeignKey<Usuario>(u => u.PersonaId);
        b.HasIndex(u => u.PersonaId).IsUnique();
    }
}
