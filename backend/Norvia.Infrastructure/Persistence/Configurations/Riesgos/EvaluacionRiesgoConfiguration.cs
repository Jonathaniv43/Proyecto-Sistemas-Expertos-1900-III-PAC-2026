using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Configurations.Riesgos;

/// <summary>El nivel (Probabilidad × Severidad) se calcula; no es columna (RN-15, RN-16).</summary>
public class EvaluacionRiesgoConfiguration : IEntityTypeConfiguration<EvaluacionRiesgo>
{
    public void Configure(EntityTypeBuilder<EvaluacionRiesgo> b)
    {
        b.HasKey(e => e.Id);
        b.TablaDeEmpresa();

        b.EnumComoTexto(e => e.Tipo, 10);
        b.Check("Probabilidad", "\"Probabilidad\" BETWEEN 1 AND 5");
        b.Check("Severidad", "\"Severidad\" BETWEEN 1 AND 5");
        b.Property(e => e.EvaluadoEn).FechaDelServidor();

        b.HasOne(e => e.Empresa).WithMany().HasForeignKey(e => e.EmpresaId);

        b.HasOne(e => e.RiesgoEmpresa)
            .WithMany(r => r.Evaluaciones)
            .HasForeignKey(e => new { e.EmpresaId, e.RiesgoEmpresaId })
            .HasPrincipalKey(r => new { r.EmpresaId, r.Id });

        b.HasOne(e => e.EvaluadoPor)
            .WithMany()
            .HasForeignKey(e => new { e.EmpresaId, e.EvaluadoPorId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });

        // Última evaluación de cada riesgo (§7).
        b.HasIndex(e => new { e.RiesgoEmpresaId, e.EvaluadoEn }).IsDescending(false, true);
    }
}
