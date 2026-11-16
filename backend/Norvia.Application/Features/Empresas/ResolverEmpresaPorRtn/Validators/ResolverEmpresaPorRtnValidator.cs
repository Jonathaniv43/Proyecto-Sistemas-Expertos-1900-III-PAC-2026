using FluentValidation;
using Norvia.Application.Features.Empresas.ResolverEmpresaPorRtn.DTOs;

namespace Norvia.Application.Features.Empresas.ResolverEmpresaPorRtn.Validators;

public sealed class ResolverEmpresaPorRtnValidator : AbstractValidator<ResolverEmpresaPorRtnRequestDto>
{
    public ResolverEmpresaPorRtnValidator()
    {
        RuleFor(x => x.Rtn)
            .NotEmpty().WithMessage("El RTN es obligatorio.")
            .Matches("^[0-9]{14}$").WithMessage("El RTN debe contener exactamente 14 dígitos numéricos.");
    }
}