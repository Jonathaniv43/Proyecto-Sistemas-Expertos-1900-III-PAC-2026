using System.Text.Json;

namespace Norvia.Application.Features.Auditoria.ListarAuditoria.DTOs;

public sealed record RegistroAuditoriaResponseDto(
    Guid Id,
    DateTime Fecha,
    string Accion,
    string Entidad,
    Guid EntidadId,
    string ActorTipo,
    Guid? VinculoEmpresaId,
    Guid? StaffPlataformaId,
    string ActorNombre,
    string? ActorCorreo,
    JsonElement? Detalle);

public sealed record AuditoriaPaginadaResponseDto(
    IReadOnlyList<RegistroAuditoriaResponseDto> Items,
    int Pagina,
    int Tamano,
    int Total,
    int TotalPaginas);
