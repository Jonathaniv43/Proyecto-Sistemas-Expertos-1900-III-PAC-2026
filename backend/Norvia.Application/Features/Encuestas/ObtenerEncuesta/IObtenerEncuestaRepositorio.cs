using Norvia.Application.Features.Encuestas.DTOs;
using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Application.Features.Encuestas.ObtenerEncuesta;

public interface IObtenerEncuestaRepositorio
{
    /// <remarks>Incluye Versiones (publicada y borrador), Preguntas y Opciones.</remarks>
    Task<Encuesta?> BuscarAsync(Guid id, CancellationToken ct);

    /// <summary>Resumen de todas las versiones publicadas o reemplazadas de la encuesta, de la más reciente a la más antigua.</summary>
    Task<IReadOnlyList<VersionEncuestaResumenResponseDto>> ListarVersionesPublicadasAsync(Guid encuestaId, CancellationToken ct);
}
