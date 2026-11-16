using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Application.Features.Encuestas.ResponderEncuesta;

public interface IResponderEncuestaRepositorio
{
    Task<Encuesta?> BuscarConVersionPublicadaAsync(Guid encuestaId, CancellationToken ct);
    Task<bool> YaParticipoAsync(Guid encuestaId, Guid vinculoEmpresaId, CancellationToken ct);
    void Agregar(RespuestaEncuesta respuesta);
    void Agregar(ParticipacionEncuesta participacion);
}
