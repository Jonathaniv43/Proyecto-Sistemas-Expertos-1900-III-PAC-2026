using Norvia.Application.Features.Auditoria.ListarAuditoria.DTOs;

namespace Norvia.Application.Features.Auditoria.ListarAuditoria;

public interface IListarAuditoriaRepositorio
{
    Task<AuditoriaPaginadaResponseDto> ListarAsync(
        string? buscar,
        string? entidad,
        string? accion,
        DateTime? desdeUtc,
        DateTime? hastaUtcExclusivo,
        int pagina,
        int tamano,
        CancellationToken ct);
}
