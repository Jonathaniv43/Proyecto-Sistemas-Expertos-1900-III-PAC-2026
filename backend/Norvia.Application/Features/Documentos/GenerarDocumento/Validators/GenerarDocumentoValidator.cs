using FluentValidation;
using Norvia.Application.Features.Documentos.GenerarDocumento.DTOs;

namespace Norvia.Application.Features.Documentos.GenerarDocumento.Validators;

public sealed class GenerarDocumentoValidator : AbstractValidator<GenerarDocumentoRequestDto>
{
    public GenerarDocumentoValidator()
    {
        RuleFor(x => x.PlantillaDocumentoId).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Elija una plantilla.")
            .NotEqual(Guid.Empty).WithMessage("Elija una plantilla.");
        RuleFor(x => x.Titulo).MaximumLength(200).WithMessage("El título no puede exceder 200 caracteres.");
    }
}
