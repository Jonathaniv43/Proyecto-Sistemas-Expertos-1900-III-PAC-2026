using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Acceso;

public class PersonaConfiguration : IEntityTypeConfiguration<Persona>
{
    public void Configure(EntityTypeBuilder<Persona> b)
    {
        b.HasKey(p => p.Id);
        b.Property(p => p.PrimerNombre).HasMaxLength(80).IsRequired();
        b.Property(p => p.SegundoNombre).HasMaxLength(80);
        b.Property(p => p.PrimerApellido).HasMaxLength(80).IsRequired();
        b.Property(p => p.SegundoApellido).HasMaxLength(80);
        b.EnumComoTexto(p => p.TipoDocumento, 15);
        b.Property(p => p.NumeroDocumento).HasMaxLength(50);
        b.Property(p => p.Telefono).HasMaxLength(30);
        b.Property(p => p.CreadoEn).FechaDelServidor();

        // UQ (TipoDocumento, NumeroDocumento) cuando ambos tienen valor.
        b.HasIndex(p => new { p.TipoDocumento, p.NumeroDocumento })
            .IsUnique()
            .HasFilter("\"TipoDocumento\" IS NOT NULL AND \"NumeroDocumento\" IS NOT NULL");
    }
}
