using Norvia.Application.Features.Plan.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Plan.ListarTareas.DTOs;

/// <summary>Filtros de la lista del plan (HU-046), como parámetros de la consulta. Todos son opcionales.</summary>
public sealed record FiltrosTareasDto
{
    public EstadoTarea? Estado { get; init; }

    /// <summary>true: pendientes o en progreso con la fecha límite antes de hoy (fecha de Honduras).</summary>
    public bool? Vencidas { get; init; }

    public Guid? ResponsableId { get; init; }

    /// <summary>true: solo las que todavía no tienen responsable.</summary>
    public bool? SinResponsable { get; init; }

    /// <summary><c>riesgo</c>, <c>accidente</c> o <c>manual</c> (RN-23).</summary>
    public OrigenTarea? Origen { get; init; }

    /// <summary>1 alta, 2 media, 3 baja.</summary>
    public short? Prioridad { get; init; }

    /// <summary>Fecha límite desde y hasta (incluidas). Las tareas sin fecha quedan fuera si se usa alguna.</summary>
    public DateOnly? FechaDesde { get; init; }
    public DateOnly? FechaHasta { get; init; }
}
