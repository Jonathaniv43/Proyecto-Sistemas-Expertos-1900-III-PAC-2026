using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Eventos.DTOs;
using Norvia.Application.Features.Eventos.ListarMedidasCorrectivas;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Eventos;

public sealed class ListarMedidasCorrectivasRepositorio(NorviaDbContext db) : IListarMedidasCorrectivasRepositorio
{
    public Task<bool> ExisteAsync(Guid eventoId, CancellationToken ct) => db.Accidente.AnyAsync(a => a.Id == eventoId, ct);

    public Task<List<MedidaCorrectivaResponseDto>> ListarAsync(Guid eventoId, CancellationToken ct) =>
        ProyeccionMedida.ListarAsync(db.Tarea.Where(t => t.AccidenteId == eventoId), ct);
}
