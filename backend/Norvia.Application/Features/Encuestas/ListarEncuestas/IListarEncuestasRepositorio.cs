using Norvia.Application.Features.Encuestas.ListarEncuestas.DTOs;

namespace Norvia.Application.Features.Encuestas.ListarEncuestas;

public interface IListarEncuestasRepositorio
{
    Task<IReadOnlyList<EncuestaResumenResponseDto>> ListarAsync(CancellationToken ct);
}
