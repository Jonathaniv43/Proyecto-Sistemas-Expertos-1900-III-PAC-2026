using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Notificaciones.ContarNoLeidas;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Notificaciones;

public sealed class ContarNoLeidasRepositorio(NorviaDbContext db) : IContarNoLeidasRepositorio
{
    public Task<int> ContarAsync(Guid destinatarioId, CancellationToken ct) =>
        ConsultasNotificacion.NoLeidas(db, destinatarioId).CountAsync(ct);
}
