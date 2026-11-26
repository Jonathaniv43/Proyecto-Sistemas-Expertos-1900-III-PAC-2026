using FluentValidation;
using Norvia.Application.Features.Eventos.ReportarEvento.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Eventos.ReportarEvento.Validators;

public sealed class ReportarEventoValidator : AbstractValidator<ReportarEventoRequestDto>
{
    public ReportarEventoValidator()
    {
        RuleFor(x => x.Tipo)
            .NotNull().WithMessage("Indique el tipo de evento.")
            .IsInEnum();

        // RN-26: EstadoEnfermedad solo aplica y es obligatorio cuando Tipo == enfermedad_profesional.
        RuleFor(x => x.EstadoEnfermedad)
            .NotNull().WithMessage("Indique si la enfermedad profesional es sospechosa o diagnosticada.")
            .IsInEnum()
            .When(x => x.Tipo == TipoAccidente.EnfermedadProfesional);

        RuleFor(x => x.EstadoEnfermedad)
            .Null().WithMessage("El estado de enfermedad solo aplica cuando el tipo es enfermedad_profesional.")
            .When(x => x.Tipo.HasValue && x.Tipo != TipoAccidente.EnfermedadProfesional);

        RuleFor(x => x.FechaHoraEvento)
            .NotNull().WithMessage("Indique la fecha y hora del evento.")
            .Must(f => f is null || f.Value != default)
            .WithMessage("Indique una fecha y hora válida para el evento.");

        RuleFor(x => x.Lugar)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Lugar));

        RuleFor(x => x.Descripcion)
            .Must(d => !string.IsNullOrWhiteSpace(d))
            .WithMessage("Describa el evento ocurrido.");
    }
}
