using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Application.Features.Encuestas.ListarEncuestasPendientes;

public interface IListarEncuestasPendientesRepositorio
{
    Task<List<Encuesta>> ListarPublicadasSinParticipacionAsync(Guid vinculoEmpresaId, CancellationToken ct);
}
