using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Encuestas.ListarEncuestas.DTOs;

namespace Norvia.Application.Features.Encuestas.ListarEncuestas;

public sealed class ListarEncuestasServicio(IListarEncuestasRepositorio repositorio) : IServicioCasoDeUso
{
    public Task<IReadOnlyList<EncuestaResumenResponseDto>> EjecutarAsync(CancellationToken ct) =>
        repositorio.ListarAsync(ct);
}
