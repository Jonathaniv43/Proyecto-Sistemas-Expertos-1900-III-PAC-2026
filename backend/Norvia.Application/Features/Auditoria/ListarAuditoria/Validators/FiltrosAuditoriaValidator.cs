using FluentValidation;
using Norvia.Application.Common.Auditoria;
using Norvia.Application.Features.Auditoria.ListarAuditoria.DTOs;

namespace Norvia.Application.Features.Auditoria.ListarAuditoria.Validators;

public sealed class FiltrosAuditoriaValidator : AbstractValidator<FiltrosAuditoriaDto>
{
    public FiltrosAuditoriaValidator()
    {
        RuleFor(x => x.Buscar).MaximumLength(100);
        RuleFor(x => x.Entidad)
            .Must(e => string.IsNullOrWhiteSpace(e) || EntidadesAuditoria.Todas.Contains(e.Trim(), StringComparer.Ordinal))
            .WithMessage("La entidad indicada no es válida para filtrar auditoría.");
        RuleFor(x => x.Accion).MaximumLength(50);
        RuleFor(x => x)
            .Must(x => !x.Desde.HasValue || !x.Hasta.HasValue || x.Hasta.Value >= x.Desde.Value)
            .WithMessage("La fecha final no puede ser anterior a la inicial.");
        RuleFor(x => x.Pagina).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Tamano).InclusiveBetween(1, 100);
    }
}
