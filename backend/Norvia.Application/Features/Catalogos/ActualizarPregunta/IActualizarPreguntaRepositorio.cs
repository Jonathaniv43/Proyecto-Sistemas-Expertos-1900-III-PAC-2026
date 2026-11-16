using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.ActualizarPregunta;

public interface IActualizarPreguntaRepositorio
{
    /// <summary>Incluye las opciones actuales.</summary>
    Task<PreguntaExploratoria?> BuscarAsync(Guid id, CancellationToken ct);
    Task<bool> CategoriaActivaExisteAsync(Guid categoriaRiesgoId, CancellationToken ct);
    Task<bool> PeligroActivoExisteAsync(Guid peligroId, CancellationToken ct);
    Task<short> UltimoOrdenAsync(Guid categoriaRiesgoId, CancellationToken ct);
    Task<bool> OrdenOcupadoAsync(Guid categoriaRiesgoId, short orden, Guid excepto, CancellationToken ct);

    /// <summary>
    /// Sustituye las opciones. Nada fuera del catálogo apunta a OpcionPregunta (las encuestas copian el texto
    /// en EncuestaOpcion), así que borrar las anteriores no afecta encuestas ya generadas.
    /// </summary>
    void ReemplazarOpciones(PreguntaExploratoria pregunta, IEnumerable<OpcionPregunta> nuevas);
}
