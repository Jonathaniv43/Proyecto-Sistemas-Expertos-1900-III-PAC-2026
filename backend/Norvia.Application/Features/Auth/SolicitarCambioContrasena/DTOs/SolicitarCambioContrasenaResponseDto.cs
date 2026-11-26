namespace Norvia.Application.Features.Auth.SolicitarCambioContrasena.DTOs;

/// <summary>Respuesta genérica (siempre igual) para no revelar si el correo existe.</summary>
public sealed record SolicitarCambioContrasenaResponseDto(string Mensaje);
