using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Notificaciones;

namespace Norvia.Infrastructure.Persistence.Configurations.Notificaciones;

/// <summary>Notificación y bandeja de salida del correo (D28, RN-75, RN-77–RN-79; 12-modelo-datos § Notificacion).</summary>
public class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> b)
    {
        b.HasKey(n => n.Id);
        b.TablaDeEmpresa();

        b.EnumComoTexto(n => n.Tipo, 20);
        b.Property(n => n.Titulo).HasMaxLength(200).IsRequired();
        b.Property(n => n.Mensaje).IsRequired();
        b.Property(n => n.CreadaEn).FechaDelServidor();
        b.EnumComoTexto(n => n.CorreoEstado, 10);
        b.Property(n => n.CorreoIntentos).HasDefaultValue((short)0);

        b.HasOne(n => n.Empresa).WithMany().HasForeignKey(n => n.EmpresaId);
        b.HasOne(n => n.Destinatario).WithMany()
            .HasForeignKey(n => new { n.EmpresaId, n.DestinatarioId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });
        b.HasOne(n => n.Tarea).WithMany()
            .HasForeignKey(n => new { n.EmpresaId, n.TareaId })
            .HasPrincipalKey(t => new { t.EmpresaId, t.Id });
        b.HasOne(n => n.VersionDocumento).WithMany()
            .HasForeignKey(n => new { n.EmpresaId, n.VersionDocumentoId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });

        // El origen corresponde al tipo: una política tiene versión y no tarea; una tarea, al revés.
        b.Check("Origen",
            "(\"Tipo\" = 'politica_publicada' AND \"VersionDocumentoId\" IS NOT NULL AND \"TareaId\" IS NULL) OR " +
            "(\"Tipo\" = 'tarea_asignada' AND \"TareaId\" IS NOT NULL AND \"VersionDocumentoId\" IS NULL)");
        b.Check("CorreoIntentos", "\"CorreoIntentos\" >= 0");
        b.Check("CorreoEnviadoEn", "(\"CorreoEnviadoEn\" IS NOT NULL) = (\"CorreoEstado\" = 'enviado')");

        // RN-75: aprobar una versión no avisa dos veces a la misma persona (HU-116).
        b.HasIndex(n => new { n.VersionDocumentoId, n.DestinatarioId })
            .IsUnique()
            .HasFilter("\"VersionDocumentoId\" IS NOT NULL");

        // Mis notificaciones, las no leídas primero (HU-117).
        b.HasIndex(n => new { n.EmpresaId, n.DestinatarioId, n.LeidaEn });

        // Correos por enviar o reintentar (HU-118): solo los pendientes, por el momento del próximo intento.
        b.HasIndex(n => n.CorreoProximoIntentoEn)
            .HasFilter("\"CorreoEstado\" = 'pendiente'");
    }
}
