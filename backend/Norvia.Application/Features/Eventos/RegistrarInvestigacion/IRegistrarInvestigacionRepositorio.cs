using Norvia.Application.Features.Eventos.DTOs;
using Norvia.Domain.Entities.Accidentes;

namespace Norvia.Application.Features.Eventos.RegistrarInvestigacion;

public interface IRegistrarInvestigacionRepositorio
{
    /// <summary>El evento de la empresa de la sesión con su investigación (con seguimiento); null si no existe.</summary>
    Task<Accidente?> BuscarAsync(Guid id, CancellationToken ct);

    /// <summary>true si el vínculo es de la empresa de la sesión, está activo y su rol puede investigar.</summary>
    Task<bool> EsInvestigadorValidoAsync(Guid vinculoId, CancellationToken ct);

    void Agregar(InvestigacionAccidente investigacion);

    Task<EventoDetalleResponseDto> DetalleAsync(Guid id, CancellationToken ct);
}
