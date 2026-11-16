using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.CrearPregunta;

public interface ICrearPreguntaRepositorio
{
    Task<bool> CategoriaActivaExisteAsync(Guid categoriaRiesgoId, CancellationToken ct);
    Task<bool> PeligroActivoExisteAsync(Guid peligroId, CancellationToken ct);

    /// <summary>Mayor <c>Orden</c> de las preguntas de la categoría; 0 si no tiene.</summary>
    Task<short> UltimoOrdenAsync(Guid categoriaRiesgoId, CancellationToken ct);

    /// <summary>Agrega la pregunta junto con sus opciones.</summary>
    void Agregar(PreguntaExploratoria pregunta);
}
