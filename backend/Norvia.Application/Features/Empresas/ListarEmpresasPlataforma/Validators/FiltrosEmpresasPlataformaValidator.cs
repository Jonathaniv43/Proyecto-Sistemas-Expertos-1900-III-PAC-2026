using FluentValidation;
using Norvia.Application.Features.Empresas.ListarEmpresasPlataforma.DTOs;

namespace Norvia.Application.Features.Empresas.ListarEmpresasPlataforma.Validators;

public sealed class FiltrosEmpresasPlataformaValidator : AbstractValidator<FiltrosEmpresasPlataformaDto>
{
    public FiltrosEmpresasPlataformaValidator()
    {
        RuleFor(x => x.Buscar).MaximumLength(100);
    }
}
