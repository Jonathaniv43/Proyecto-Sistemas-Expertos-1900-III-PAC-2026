using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Documentos;

namespace Norvia.Infrastructure.Persistence.Configurations.Documentos;

/// <summary>
/// Versión de un documento (D13, RN-49 a RN-51; 12-modelo-datos § VersionDocumento). Sin disparador: la aplicación
/// solo edita la versión en borrador (RN-50), y la base garantiza una vigente y un borrador por documento.
/// </summary>
public class VersionDocumentoConfiguration : IEntityTypeConfiguration<VersionDocumento>
{
    public void Configure(EntityTypeBuilder<VersionDocumento> b)
    {
        b.HasKey(v => v.Id);
        b.TablaDeEmpresa();

        b.Property(v => v.Contenido).IsRequired();
        b.EnumComoTexto(v => v.Estado, 11);
        b.Property(v => v.CreadaEn).FechaDelServidor();

        b.HasOne(v => v.Empresa).WithMany().HasForeignKey(v => v.EmpresaId);
        b.HasOne(v => v.Documento).WithMany(d => d.Versiones)
            .HasForeignKey(v => new { v.EmpresaId, v.DocumentoId })
            .HasPrincipalKey(d => new { d.EmpresaId, d.Id });
        b.HasOne(v => v.CreadaPor).WithMany()
            .HasForeignKey(v => new { v.EmpresaId, v.CreadaPorId })
            .HasPrincipalKey(p => new { p.EmpresaId, p.Id });
        b.HasOne(v => v.AprobadaPor).WithMany()
            .HasForeignKey(v => new { v.EmpresaId, v.AprobadaPorId })
            .HasPrincipalKey(p => new { p.EmpresaId, p.Id });

        b.Check("NumeroVersion", "\"NumeroVersion\" >= 1");
        // Una versión aprobada (vigente o reemplazada) registra quién la aprobó y cuándo (HU-072).
        b.Check("Aprobacion",
            "\"Estado\" = 'borrador' OR (\"AprobadaPorId\" IS NOT NULL AND \"AprobadaEn\" IS NOT NULL)");

        b.HasIndex(v => new { v.DocumentoId, v.NumeroVersion }).IsUnique();
        // Dos índices sobre la misma columna: llevan nombre para que EF no los junte en uno.
        // RN-51: una sola versión vigente por documento.
        b.HasIndex(v => v.DocumentoId, "IX_VersionDocumento_DocumentoId_Vigente")
            .IsUnique()
            .HasFilter("\"Estado\" = 'vigente'");
        // Un solo borrador a la vez.
        b.HasIndex(v => v.DocumentoId, "IX_VersionDocumento_DocumentoId_Borrador")
            .IsUnique()
            .HasFilter("\"Estado\" = 'borrador'");
    }
}
