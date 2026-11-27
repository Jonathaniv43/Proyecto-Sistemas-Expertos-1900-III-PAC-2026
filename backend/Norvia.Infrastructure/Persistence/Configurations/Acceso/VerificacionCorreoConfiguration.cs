using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;

namespace Norvia.Infrastructure.Persistence.Configurations.Acceso;

public class VerificacionCorreoConfiguration : IEntityTypeConfiguration<VerificacionCorreo>
{
    public void Configure(EntityTypeBuilder<VerificacionCorreo> b)
    {
        b.HasKey(v => v.Id);
        b.EnumComoTexto(v => v.Proposito, 20);
        b.Property(v => v.Email).HasMaxLength(254).IsRequired();
        b.Property(v => v.ProveedorUserId).HasMaxLength(255);
        b.Property(v => v.CodigoHash).HasMaxLength(64).IsRequired(); // SHA-256 en hexadecimal
        b.Property(v => v.Intentos).PorDefectoCero();
        b.Property(v => v.EnviosEnVentana).PorDefectoCero();
        b.Property(v => v.CreadoEn).FechaDelServidor();

        b.HasOne(v => v.Acceso).WithMany(a => a.VerificacionesCorreo).HasForeignKey(v => v.AccesoId);

        // RN-86: una fila por correo y cuenta del proveedor (Microsoft); reenviar actualiza la misma fila.
        b.HasIndex(v => new { v.Email, v.ProveedorUserId })
            .IsUnique()
            .HasFilter("\"ProveedorUserId\" IS NOT NULL");

        // Una fila de activación pendiente por Acceso; reenviar actualiza la misma fila.
        b.HasIndex(v => v.AccesoId)
            .IsUnique()
            .HasFilter("\"AccesoId\" IS NOT NULL");

        // El propósito corresponde a la columna que lo identifica: Microsoft tiene ProveedorUserId y no AccesoId; activación, al revés.
        b.Check("Proposito",
            "(\"Proposito\" = 'microsoft' AND \"ProveedorUserId\" IS NOT NULL AND \"AccesoId\" IS NULL) OR " +
            "(\"Proposito\" = 'activacion_cuenta' AND \"AccesoId\" IS NOT NULL AND \"ProveedorUserId\" IS NULL)");
    }
}
