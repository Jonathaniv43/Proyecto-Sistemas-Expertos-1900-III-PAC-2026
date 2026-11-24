using FluentValidation;
using Norvia.Application.Features.Plan.ListarTareas.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Plan.ListarTareas.Validators;

public sealed class FiltrosTareasValidator : AbstractValidator<FiltrosTareasDto>
{
    public FiltrosTareasValidator()
    {
        RuleFor(x => x.Prioridad).InclusiveBetween((short)1, (short)3).When(x => x.Prioridad is not null)
            .WithMessage("La prioridad va de 1 (alta) a 3 (baja).");
        RuleFor(x => x).Must(x => !(x.ResponsableId is not null && x.SinResponsable == true))
            .WithName("Responsable").WithMessage("Elija un responsable o las tareas sin responsable, no los dos.");
        RuleFor(x => x).Must(x => !(x.Vencidas == true && x.Estado is EstadoTarea.Completada or EstadoTarea.Cancelada))
            .WithName("Vencidas").WithMessage("Solo las tareas pendientes o en progreso pueden estar vencidas.");
        RuleFor(x => x).Must(x => x.FechaDesde is null || x.FechaHasta is null || x.FechaDesde <= x.FechaHasta)
            .WithName("FechaHasta").WithMessage("La fecha final no puede ser anterior a la inicial.");
    }
}
