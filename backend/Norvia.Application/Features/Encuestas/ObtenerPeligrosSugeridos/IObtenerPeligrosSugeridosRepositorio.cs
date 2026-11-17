using Norvia.Domain.Entities.Riesgos;

namespace Norvia.Application.Features.Encuestas.ObtenerPeligrosSugeridos;

/// <summary>Una respuesta que eligió una opción de exposición en una pregunta con peligro.</summary>
public sealed record ExposicionRespondida(Guid RespuestaId, Guid PreguntaId, string PreguntaTexto, Guid PeligroId);

public interface IObtenerPeligrosSugeridosRepositorio
{
    Task<bool> ExisteEncuestaAsync(Guid encuestaId, CancellationToken ct);

    /// <summary>Respuestas de todas las versiones de la encuesta.</summary>
    Task<int> ContarRespuestasAsync(Guid encuestaId, CancellationToken ct);

    /// <summary>
    /// Exposiciones de todas las respuestas de la encuesta, en selección única y múltiple. Usa la copia de la
    /// versión contestada (peligro de la pregunta y marca de exposición de la opción), no el catálogo actual.
    /// </summary>
    Task<List<ExposicionRespondida>> ListarExposicionesAsync(Guid encuestaId, CancellationToken ct);

    Task<Dictionary<Guid, string>> NombresDePeligrosAsync(IReadOnlyCollection<Guid> peligroIds, CancellationToken ct);

    /// <summary>Riesgos ya registrados a partir de esta encuesta, en cualquier estado.</summary>
    Task<List<RiesgoEmpresa>> ListarRiesgosDeEncuestaAsync(Guid encuestaId, CancellationToken ct);

    void AgregarRiesgo(RiesgoEmpresa riesgo);
}
