using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Accidentes;

namespace Norvia.Infrastructure.Persistence.Configurations.Accidentes;

public class AccidenteConfiguration : IEntityTypeConfiguration<Accidente>
{
    public void Configure(EntityTypeBuilder<Accidente> b)
    {
        b.HasKey(a => a.Id);
        b.TablaDeEmpresa();

        b.EnumComoTexto(a => a.Tipo, 25);
        b.EnumComoTexto(a => a.EstadoEnfermedad, 15);
        b.Property(a => a.EsGrave).PorDefectoFalse();
        b.Property(a => a.Lugar).HasMaxLength(200);
        b.Property(a => a.Descripcion).IsRequired();
        b.Property(a => a.RegistradoEn).FechaDelServidor();
        b.EnumComoTexto(a => a.Estado, 16);

        b.HasOne(a => a.Empresa).WithMany().HasForeignKey(a => a.EmpresaId);

        // FK compuesta para quien descarta el evento (D15, HU-049).
        b.HasOne(a => a.DescartadoPor)
            .WithMany()
            .HasForeignKey(a => new { a.EmpresaId, a.DescartadoPorId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });

        // RN-26: EstadoEnfermedad es obligatorio y exclusivo de enfermedad_profesional.
        b.Check("EstadoEnfermedadSegunTipo", "(\"EstadoEnfermedad\" IS NOT NULL) = (\"Tipo\" = 'enfermedad_profesional')");

        // Integridad del cierre y del descarte (RN-28, HU-049).
        b.Check("CerradoEn", "(\"CerradoEn\" IS NOT NULL) = (\"Estado\" = 'cerrado')");
        b.Check("Descarte", "(\"DescartadoPorId\" IS NOT NULL AND \"DescartadoEn\" IS NOT NULL) = (\"Estado\" = 'descartado')");
        b.Check("MotivoDescarte", "\"MotivoDescarte\" IS NULL OR \"Estado\" = 'descartado'");

        // Consultas y estadísticas por empresa y fecha (§7).
        b.HasIndex(a => new { a.EmpresaId, a.FechaHoraEvento });
    }
}
