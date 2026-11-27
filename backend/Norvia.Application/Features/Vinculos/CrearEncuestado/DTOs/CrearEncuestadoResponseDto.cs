using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Vinculos.CrearEncuestado.DTOs;

/// <param name="CuentaNueva">true si se creó la cuenta: le llegó un código de 6 dígitos para activarla.</param>
/// <param name="CorreoActivacionEnviado">Solo tiene sentido si <paramref name="CuentaNueva"/> es true.</param>
public sealed record CrearEncuestadoResponseDto(
    Guid Id,
    Guid PersonaId,
    string Email,
    string NombreCompleto,
    string RolCodigo,
    EstadoVinculo Estado,
    DateTime CreadoEn,
    bool CuentaNueva,
    bool CorreoActivacionEnviado);
