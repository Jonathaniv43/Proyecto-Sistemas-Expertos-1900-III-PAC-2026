using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Vinculos.CrearEncuestado.DTOs;

/// <param name="PasswordTemporal">
/// Solo si Norvia generó la contraseña (cuenta nueva sin <c>passwordTemporal</c>). Se muestra una única vez:
/// el administrador la entrega a la persona, que debe cambiarla al entrar.
/// </param>
public sealed record CrearEncuestadoResponseDto(
    Guid Id,
    Guid PersonaId,
    string Email,
    string NombreCompleto,
    string RolCodigo,
    EstadoVinculo Estado,
    DateTime CreadoEn,
    bool CuentaNueva,
    string? PasswordTemporal);
