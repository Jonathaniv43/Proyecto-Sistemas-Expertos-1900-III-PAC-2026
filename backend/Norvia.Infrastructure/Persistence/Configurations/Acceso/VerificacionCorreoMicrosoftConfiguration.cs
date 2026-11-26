using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;

namespace Norvia.Infrastructure.Persistence.Configurations.Acceso;

public class VerificacionCorreoMicrosoftConfiguration : IEntityTypeConfiguration<VerificacionCorreoMicrosoft>
{
    public void Configure(EntityTypeBuilder<VerificacionCorreoMicrosoft> b)
    {
        b.HasKey(v => v.Id);
        b.Property(v => v.Email).HasMaxLength(254).IsRequired();
        b.Property(v => v.ProveedorUserId).HasMaxLength(255).IsRequired();
        b.Property(v => v.CodigoHash).HasMaxLength(64).IsRequired(); // SHA-256 en hexadecimal
        b.Property(v => v.Intentos).PorDefectoCero();
        b.Property(v => v.EnviosEnVentana).PorDefectoCero();
        b.Property(v => v.CreadoEn).FechaDelServidor();

        // RN-86: una fila por correo y cuenta del proveedor; reenviar actualiza la misma fila.
        b.HasIndex(v => new { v.Email, v.ProveedorUserId }).IsUnique();
    }
}
