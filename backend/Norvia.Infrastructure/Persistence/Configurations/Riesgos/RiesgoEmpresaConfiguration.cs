using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Riesgos;

public class RiesgoEmpresaConfiguration : IEntityTypeConfiguration<RiesgoEmpresa>
{
    public void Configure(EntityTypeBuilder<RiesgoEmpresa> b)
    {
        b.HasKey(r => r.Id);
        b.TablaDeEmpresa();

        b.Property(r => r.DescripcionPeligro).HasMaxLength(300);
        b.Property(r => r.Actividad).HasMaxLength(200);
        b.EnumComoTexto(r => r.Estado, 12);
        b.Property(r => r.CreadoEn).FechaDelServidor();

        b.HasOne(r => r.Empresa).WithMany().HasForeignKey(r => r.EmpresaId);
        b.HasOne(r => r.Peligro).WithMany().HasForeignKey(r => r.PeligroId); // Peligro es global: FK simple

        // FK compuesta: el revisor debe ser de la misma empresa (D15). Sin áreas: el riesgo es de la empresa (D4).
        b.HasOne(r => r.RevisadoPor)
            .WithMany()
            .HasForeignKey(r => new { r.EmpresaId, r.RevisadoPorId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });

        // EncuestaId queda sin FK hasta que exista Encuesta (M3).

        b.Check("PeligroODescripcion", "\"PeligroId\" IS NOT NULL OR \"DescripcionPeligro\" IS NOT NULL");
        b.Check("SugeridoConOrigen",
            "\"Estado\" <> 'sugerido' OR (\"EncuestaId\" IS NOT NULL AND \"PeligroId\" IS NOT NULL)");

        // Calcular las sugerencias de nuevo no las duplica.
        b.HasIndex(r => new { r.EncuestaId, r.PeligroId })
            .IsUnique()
            .HasFilter("\"EncuestaId\" IS NOT NULL");

        // Matriz de la empresa (§7).
        b.HasIndex(r => new { r.EmpresaId, r.Estado });
    }
}
