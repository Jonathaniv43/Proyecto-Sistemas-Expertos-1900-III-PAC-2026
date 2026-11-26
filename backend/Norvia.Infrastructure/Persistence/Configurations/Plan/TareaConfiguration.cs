using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Norvia.Domain.Entities.Plan;

namespace Norvia.Infrastructure.Persistence.Configurations.Plan;

/// <summary>Tarea del plan (D6, 12-modelo-datos § Tarea).</summary>
public class TareaConfiguration : IEntityTypeConfiguration<Tarea>
{
    public void Configure(EntityTypeBuilder<Tarea> b)
    {
        b.HasKey(t => t.Id);
        b.TablaDeEmpresa();

        b.Property(t => t.Titulo).HasMaxLength(200).IsRequired();
        b.Property(t => t.Descripcion);
        b.EnumComoTexto(t => t.Estado, 12);
        b.Property(t => t.RequiereEvidencia).PorDefectoFalse();
        b.Property(t => t.CreadaEn).FechaDelServidor();

        b.HasOne(t => t.Empresa).WithMany().HasForeignKey(t => t.EmpresaId);

        b.HasOne(t => t.Responsable).WithMany()
            .HasForeignKey(t => new { t.EmpresaId, t.ResponsableId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });
        b.HasOne(t => t.CreadaPor).WithMany()
            .HasForeignKey(t => new { t.EmpresaId, t.CreadaPorId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.Id });
        b.HasOne(t => t.PlantillaTarea).WithMany().HasForeignKey(t => t.PlantillaTareaId);
        b.HasOne(t => t.RiesgoEmpresa).WithMany()
            .HasForeignKey(t => new { t.EmpresaId, t.RiesgoEmpresaId })
            .HasPrincipalKey(r => new { r.EmpresaId, r.Id });
        // El control debe ser del mismo riesgo.
        b.HasOne(t => t.Control).WithMany()
            .HasForeignKey(t => new { t.RiesgoEmpresaId, t.ControlId })
            .HasPrincipalKey(c => new { c.RiesgoEmpresaId, c.Id });
        b.HasOne(t => t.Accidente).WithMany()
            .HasForeignKey(t => new { t.EmpresaId, t.AccidenteId })
            .HasPrincipalKey(a => new { a.EmpresaId, a.Id });

        b.Check("Prioridad", "\"Prioridad\" BETWEEN 1 AND 3");
        // RN-23: un solo origen.
        b.Check("UnOrigen", "\"RiesgoEmpresaId\" IS NULL OR \"AccidenteId\" IS NULL");
        b.Check("ControlConRiesgo", "\"ControlId\" IS NULL OR \"RiesgoEmpresaId\" IS NOT NULL");
        // RN-22: para iniciarse necesita responsable y fecha límite.
        b.Check("IniciadaConResponsable",
            "\"Estado\" NOT IN ('en_progreso', 'completada') OR (\"ResponsableId\" IS NOT NULL AND \"FechaLimite\" IS NOT NULL)");
        b.Check("CompletadaEn", "(\"CompletadaEn\" IS NOT NULL) = (\"Estado\" = 'completada')");

        // Generar el plan dos veces no duplica tareas (HU-042): una por plantilla de cada riesgo y una por control.
        b.HasIndex(t => new { t.RiesgoEmpresaId, t.PlantillaTareaId })
            .IsUnique()
            .HasFilter("\"RiesgoEmpresaId\" IS NOT NULL AND \"PlantillaTareaId\" IS NOT NULL");
        b.HasIndex(t => t.ControlId)
            .IsUnique()
            .HasFilter("\"ControlId\" IS NOT NULL");

        // Plan de la empresa por estado y prioridad.
        b.HasIndex(t => new { t.EmpresaId, t.Estado, t.Prioridad });
    }
}
