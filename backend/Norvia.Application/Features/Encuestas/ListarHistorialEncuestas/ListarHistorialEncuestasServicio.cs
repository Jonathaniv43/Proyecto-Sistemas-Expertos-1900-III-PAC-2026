using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Encuestas.ListarHistorialEncuestas.DTOs;

namespace Norvia.Application.Features.Encuestas.ListarHistorialEncuestas;

/// <summary>
/// HU-034: encuestas cerradas o con versiones reemplazadas (y vigentes si se solicitan para comparar).
/// </summary>
public sealed class ListarHistorialEncuestasServicio(
    IListarHistorialEncuestasRepositorio repositorio) : IServicioCasoDeUso
{
    public Task<IReadOnlyList<EncuestaHistorialResponseDto>> EjecutarAsync(
        FiltrosHistorialEncuestasDto filtros, CancellationToken ct) =>
        repositorio.ListarAsync(filtros, ct);
}
