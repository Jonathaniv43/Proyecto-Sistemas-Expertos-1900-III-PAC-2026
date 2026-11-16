namespace Norvia.Application.Features.Vinculos.CrearEncuestado.DTOs;

/// <summary>Da acceso a un trabajador con el rol fijo <c>encuestado</c> (HU-115).</summary>
public sealed record CrearEncuestadoRequestDto
{
    public string Email { get; init; } = string.Empty;
    public string PrimerNombre { get; init; } = string.Empty;
    public string? SegundoNombre { get; init; }
    public string PrimerApellido { get; init; } = string.Empty;
    public string? SegundoApellido { get; init; }

    /// <summary>Opcional. Si la cuenta es nueva y no se envía, Norvia genera una y la devuelve.</summary>
    public string? PasswordTemporal { get; init; }
}
