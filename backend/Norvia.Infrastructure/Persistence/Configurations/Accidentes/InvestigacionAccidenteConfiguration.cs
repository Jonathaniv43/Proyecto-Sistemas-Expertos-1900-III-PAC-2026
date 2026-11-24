using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Accidentes;

namespace Norvia.Infrastructure.Persistence.Configurations.Accidentes;

/// <summary>Investigación de un evento (12-modelo-datos § InvestigacionAccidente): una por evento.</summary>
public class InvestigacionAccidenteConfiguration : IEntityTypeConfiguration<InvestigacionAccidente>
{
    public void Configure(EntityTypeBuilder<InvestigacionAccidente> b)
    {
        b.HasKey(i => i.Id);
        b.TablaDeEmpresa();

        b.Property(i => i.CausasInmediatas).IsRequired();
        b.Property(i => i.CausasBasicas).IsRequired();
        b.Property(i => i.Conclusiones);

        b.HasOne(i => i.Empresa).WithMany().HasForeignKey(i => i.EmpresaId);
        b.HasOne(i => i.Accidente)
            .WithOne(a => a.Investigacion)
            .HasForeignKey<InvestigacionAccidente>(i => new { i.EmpresaId, i.AccidenteId })
            .HasPrincipalKey<Accidente>(a => new { a.EmpresaId, a.Id });
        b.HasOne(i => i.InvestigadaPor).WithMany()
            .HasForeignKey(i => new { i.EmpresaId, i.InvestigadaPorId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });

        // Una investigación por evento: la relación uno a uno crea la UQ (EmpresaId, AccidenteId).
    }
}
