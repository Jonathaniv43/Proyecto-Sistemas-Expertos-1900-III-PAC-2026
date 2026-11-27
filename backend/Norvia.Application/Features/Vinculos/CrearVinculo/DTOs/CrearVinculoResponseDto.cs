using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Vinculos.CrearVinculo.DTOs;

/// <summary>Mismos datos que <c>VinculoResponseDto</c>, más si se creó la cuenta.</summary>
/// <param name="CuentaNueva">
/// true si se creó la cuenta (debe activarla con el código que le llegó por correo);
/// false si se reutilizó una cuenta existente con su contraseña de siempre.
/// </param>
/// <param name="CorreoActivacionEnviado">Solo tiene sentido si <paramref name="CuentaNueva"/> es true.</param>
public sealed record CrearVinculoResponseDto(
    Guid Id,
    Guid PersonaId,
    string Email,
    string NombreCompleto,
    string RolCodigo,
    EstadoVinculo Estado,
    DateTime CreadoEn,
    bool CuentaNueva,
    bool CorreoActivacionEnviado);
