using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Encuestas.ListarTiposRespuesta.DTOs;

namespace Norvia.Application.Features.Encuestas.ListarTiposRespuesta;

/// <summary>Catálogo de tipos de respuesta con instrucción UI para el editor.</summary>
public sealed class ListarTiposRespuestaServicio(IListarTiposRespuestaRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<IReadOnlyList<TipoRespuestaPreguntaResponseDto>> EjecutarAsync(CancellationToken ct)
    {
        var tipos = await repositorio.ListarActivosAsync(ct);
        return tipos.Select(TipoRespuestaPreguntaResponseDto.De).ToList();
    }
}
