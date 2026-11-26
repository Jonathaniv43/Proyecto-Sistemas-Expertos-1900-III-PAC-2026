using Norvia.Application.Features.Eventos.ListarEventos.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Eventos.ListarEventos;

public interface IListarEventosRepositorio
{
    Task<IReadOnlyList<EventoResumenResponseDto>> ListarAsync(
        EstadoAccidente? estado, TipoAccidente? tipo, bool? esGrave, CancellationToken ct);
}
