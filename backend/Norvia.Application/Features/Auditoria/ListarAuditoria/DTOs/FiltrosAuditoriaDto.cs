namespace Norvia.Application.Features.Auditoria.ListarAuditoria.DTOs;

public sealed record FiltrosAuditoriaDto(
    string? Buscar = null,
    string? Entidad = null,
    string? Accion = null,
    DateOnly? Desde = null,
    DateOnly? Hasta = null,
    int Pagina = 1,
    int Tamano = 25);
