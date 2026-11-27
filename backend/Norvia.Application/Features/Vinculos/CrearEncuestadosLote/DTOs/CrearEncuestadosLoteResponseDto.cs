namespace Norvia.Application.Features.Vinculos.CrearEncuestadosLote.DTOs;

/// <param name="Fila">Posición en la lista enviada, desde 1.</param>
/// <param name="CuentaNueva">true si se creó la cuenta: le llegó un código de 6 dígitos para activarla.</param>
/// <param name="CorreoActivacionEnviado">Solo tiene sentido si <paramref name="CuentaNueva"/> es true.</param>
public sealed record EncuestadoCreadoDto(
    int Fila, Guid VinculoId, string Email, string NombreCompleto, bool CuentaNueva, bool CorreoActivacionEnviado);

public sealed record ErrorFilaDto(int Fila, string Email, string Codigo, string Mensaje);

public sealed record CrearEncuestadosLoteResponseDto(
    int Creados, IReadOnlyList<EncuestadoCreadoDto> Cuentas, IReadOnlyList<ErrorFilaDto> Errores);
