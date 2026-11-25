using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Documentos;

namespace Norvia.Infrastructure.Persistence.Configurations.Documentos;

/// <summary>Documento de la empresa (D13; 12-modelo-datos § Documento).</summary>
public class DocumentoConfiguration : IEntityTypeConfiguration<Documento>
{
    public void Configure(EntityTypeBuilder<Documento> b)
    {
        b.HasKey(d => d.Id);
        b.TablaDeEmpresa();

        b.Property(d => d.Titulo).HasMaxLength(200).IsRequired();
        b.EnumComoTexto(d => d.TipoDocumento, 15);
        b.Property(d => d.CreadoEn).FechaDelServidor();

        b.HasOne(d => d.Empresa).WithMany().HasForeignKey(d => d.EmpresaId);
        // La plantilla es global (D13): FK simple.
        b.HasOne(d => d.PlantillaDocumento).WithMany().HasForeignKey(d => d.PlantillaDocumentoId);
        b.HasOne(d => d.CreadoPor).WithMany()
            .HasForeignKey(d => new { d.EmpresaId, d.CreadoPorId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });
    }
}
