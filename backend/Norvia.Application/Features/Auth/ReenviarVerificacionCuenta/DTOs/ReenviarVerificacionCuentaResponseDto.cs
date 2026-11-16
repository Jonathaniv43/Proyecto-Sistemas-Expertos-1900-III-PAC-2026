namespace Norvia.Application.Features.Auth.ReenviarVerificacionCuenta.DTOs;

/// <summary>Respuesta genérica (siempre igual) para no revelar si el correo existe.</summary>
public sealed record ReenviarVerificacionCuentaResponseDto(string Mensaje);
