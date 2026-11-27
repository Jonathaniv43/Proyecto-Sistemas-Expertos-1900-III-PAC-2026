namespace Norvia.Application.Features.Vinculos.CrearVinculo.DTOs;

/// <summary>
/// Da acceso a la empresa de la sesión a una persona, identificada por su correo (HU-020).
/// Si el correo ya tiene cuenta en Norvia, se reutiliza la cuenta y su persona: los nombres enviados se
/// ignoran (RN-65). Si es nueva, le llega un código de 6 dígitos para activarla.
/// </summary>
public sealed record CrearVinculoRequestDto
{
    public string Email { get; init; } = string.Empty;
    public string PrimerNombre { get; init; } = string.Empty;
    public string? SegundoNombre { get; init; }
    public string PrimerApellido { get; init; } = string.Empty;
    public string? SegundoApellido { get; init; }

    /// <summary><c>admin_empresa</c>, <c>responsable_sst</c>, <c>colaborador</c> o <c>encuestado</c>.</summary>
    public string RolCodigo { get; init; } = string.Empty;
}
