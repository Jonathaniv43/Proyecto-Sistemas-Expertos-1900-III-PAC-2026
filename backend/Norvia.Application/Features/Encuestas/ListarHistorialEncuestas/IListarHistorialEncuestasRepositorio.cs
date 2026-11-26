using Norvia.Application.Features.Encuestas.ListarHistorialEncuestas.DTOs;

namespace Norvia.Application.Features.Encuestas.ListarHistorialEncuestas;

public interface IListarHistorialEncuestasRepositorio
{
    Task<IReadOnlyList<EncuestaHistorialResponseDto>> ListarAsync(
        FiltrosHistorialEncuestasDto filtros, CancellationToken ct);
}
