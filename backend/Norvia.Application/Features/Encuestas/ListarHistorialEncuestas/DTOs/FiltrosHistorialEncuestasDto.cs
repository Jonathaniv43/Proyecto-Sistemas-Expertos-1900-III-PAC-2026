namespace Norvia.Application.Features.Encuestas.ListarHistorialEncuestas.DTOs;

public sealed record FiltrosHistorialEncuestasDto(
    string? Buscar = null,
    bool IncluirVigentes = false);
