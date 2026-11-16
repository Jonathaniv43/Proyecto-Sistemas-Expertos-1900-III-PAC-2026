using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Vinculos.CrearVinculo.DTOs;

/// <summary>Mismos datos que <c>VinculoResponseDto</c>, más si se creó la cuenta.</summary>
/// <param name="CuentaNueva">
/// true si se creó la cuenta con la contraseña temporal (debe cambiarla al entrar);
/// false si se reutilizó una cuenta existente con su contraseña de siempre.
/// </param>
public sealed record CrearVinculoResponseDto(
    Guid Id,
    Guid PersonaId,
    string Email,
    string NombreCompleto,
    string RolCodigo,
    EstadoVinculo Estado,
    DateTime CreadoEn,
    bool CuentaNueva);
