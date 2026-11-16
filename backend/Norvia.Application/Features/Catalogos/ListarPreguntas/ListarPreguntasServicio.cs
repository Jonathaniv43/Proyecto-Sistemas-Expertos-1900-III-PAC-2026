using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Catalogos.DTOs;

namespace Norvia.Application.Features.Catalogos.ListarPreguntas;

/// <summary>Lista las preguntas exploratorias con sus opciones, opcionalmente solo las de una categoría.</summary>
public sealed class ListarPreguntasServicio(IListarPreguntasRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<IReadOnlyList<PreguntaResponseDto>> EjecutarAsync(Guid? categoriaRiesgoId, CancellationToken ct) =>
        (await repositorio.ListarAsync(categoriaRiesgoId, ct)).Select(PreguntaResponseDto.De).ToList();
}
