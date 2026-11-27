using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Auditoria;

namespace Norvia.Infrastructure.Persistence.Configurations.Auditoria;

/// <summary>Configuración de <see cref="RegistroAuditoria"/> (HU-099, 12-modelo-datos § RegistroAuditoria).</summary>
public class RegistroAuditoriaConfiguration : IEntityTypeConfiguration<RegistroAuditoria>
{
    public void Configure(EntityTypeBuilder<RegistroAuditoria> b)
    {
        b.HasKey(r => r.Id);
        b.TablaDeEmpresa();

        b.Property(r => r.Accion).HasMaxLength(50).IsRequired();
        b.Property(r => r.Entidad).HasMaxLength(60).IsRequired();
        b.Property(r => r.EntidadId).IsRequired();
        b.Property(r => r.Fecha).FechaDelServidor();
        b.Property(r => r.Detalle).HasColumnType("jsonb");

        b.HasOne(r => r.Empresa)
            .WithMany()
            .HasForeignKey(r => r.EmpresaId);

        b.HasOne(r => r.VinculoEmpresa)
            .WithMany()
            .HasForeignKey(r => new { r.EmpresaId, r.VinculoEmpresaId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });

        b.HasOne(r => r.StaffPlataforma)
            .WithMany()
            .HasForeignKey(r => r.StaffPlataformaId);

        b.Check("AutorExcluyente", "\"VinculoEmpresaId\" IS NULL OR \"StaffPlataformaId\" IS NULL");

        // Consultas cronológicas por empresa y rastreo puntual por entidad afectada (12-modelo-datos §7).
        b.HasIndex(r => new { r.EmpresaId, r.Fecha }).IsDescending(false, true);
        b.HasIndex(r => new { r.Entidad, r.EntidadId });
    }
}
