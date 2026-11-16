using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.ListarPreguntas;

public interface IListarPreguntasRepositorio
{
    /// <summary>Incluye las opciones de cada pregunta.</summary>
    Task<List<PreguntaExploratoria>> ListarAsync(Guid? categoriaRiesgoId, CancellationToken ct);
}
