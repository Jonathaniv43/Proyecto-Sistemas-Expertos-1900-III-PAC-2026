using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Application.Features.Encuestas.CrearEncuesta;

public interface ICrearEncuestaRepositorio
{
    Task<short?> BuscarRubroDeEmpresaAsync(Guid empresaId, CancellationToken ct);

    /// <remarks>Incluye Preguntas activas y sus Opciones.</remarks>
    Task<List<CategoriaRiesgo>> ListarCategoriasActivasConPreguntasAsync(
        short rubroId, IReadOnlyList<Guid> categoriaIds, CancellationToken ct);

    void Agregar(Encuesta encuesta);
}
