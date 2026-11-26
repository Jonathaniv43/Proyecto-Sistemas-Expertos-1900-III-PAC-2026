namespace Norvia.Application.Features.Vinculos.CrearEncuestadosLote.DTOs;

/// <param name="Fila">Posición en la lista enviada, desde 1.</param>
/// <param name="PasswordTemporal">Solo en cuentas nuevas: contraseña generada, visible una única vez.</param>
public sealed record EncuestadoCreadoDto(
    int Fila, Guid VinculoId, string Email, string NombreCompleto, bool CuentaNueva, string? PasswordTemporal);

public sealed record ErrorFilaDto(int Fila, string Email, string Codigo, string Mensaje);

public sealed record CrearEncuestadosLoteResponseDto(
    int Creados, IReadOnlyList<EncuestadoCreadoDto> Cuentas, IReadOnlyList<ErrorFilaDto> Errores);
