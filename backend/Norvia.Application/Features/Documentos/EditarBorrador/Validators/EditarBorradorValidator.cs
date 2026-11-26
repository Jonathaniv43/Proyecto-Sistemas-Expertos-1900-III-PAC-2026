using FluentValidation;
using Norvia.Application.Features.Documentos.EditarBorrador.DTOs;

namespace Norvia.Application.Features.Documentos.EditarBorrador.Validators;

public sealed class EditarBorradorValidator : AbstractValidator<EditarBorradorRequestDto>
{
    /// <summary>Holgado para una política o un procedimiento (las plantillas iniciales tienen unos 4000).</summary>
    public const int LargoMaximo = 50_000;

    public EditarBorradorValidator()
    {
        RuleFor(x => x.Contenido).Cascade(CascadeMode.Stop)
            .Must(c => !string.IsNullOrWhiteSpace(c)).WithMessage("El documento no puede quedar vacío.")
            .MaximumLength(LargoMaximo).WithMessage($"El documento no puede exceder {LargoMaximo} caracteres.");
    }
}
