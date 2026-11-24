using Norvia.Application.Features.Eventos.DTOs;

namespace Norvia.Application.Features.Eventos.ListarMedidasCorrectivas;

public interface IListarMedidasCorrectivasRepositorio
{
    Task<bool> ExisteAsync(Guid eventoId, CancellationToken ct);

    /// <summary>Las tareas con origen en el evento, también las canceladas.</summary>
    Task<List<MedidaCorrectivaResponseDto>> ListarAsync(Guid eventoId, CancellationToken ct);
}
