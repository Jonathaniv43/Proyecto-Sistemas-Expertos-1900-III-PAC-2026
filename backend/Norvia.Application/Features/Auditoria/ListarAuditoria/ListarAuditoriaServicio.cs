using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Auditoria.ListarAuditoria.DTOs;

namespace Norvia.Application.Features.Auditoria.ListarAuditoria;

/// <summary>HU-099: consulta cronológica, filtrable y paginada del registro de auditoría de la empresa.</summary>
public sealed class ListarAuditoriaServicio(IListarAuditoriaRepositorio repositorio) : IServicioCasoDeUso
{
    public Task<AuditoriaPaginadaResponseDto> EjecutarAsync(FiltrosAuditoriaDto filtros, CancellationToken ct)
    {
        var desdeUtc = filtros.Desde is { } d
            ? new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc).AddHours(6)
            : (DateTime?)null;

        var hastaUtcExclusivo = filtros.Hasta is { } h
            ? new DateTime(h.Year, h.Month, h.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1).AddHours(6)
            : (DateTime?)null;

        return repositorio.ListarAsync(
            filtros.Buscar?.Trim(),
            filtros.Entidad?.Trim(),
            filtros.Accion?.Trim(),
            desdeUtc,
            hastaUtcExclusivo,
            filtros.Pagina,
            filtros.Tamano,
            ct);
    }
}
