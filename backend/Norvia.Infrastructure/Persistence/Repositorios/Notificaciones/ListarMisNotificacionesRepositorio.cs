using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Notificaciones.DTOs;
using Norvia.Application.Features.Notificaciones.ListarMisNotificaciones;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Notificaciones;

/// <summary>El filtro global limita a la empresa de la sesión; aquí se limita al destinatario (RN-79).</summary>
public sealed class ListarMisNotificacionesRepositorio(NorviaDbContext db) : IListarMisNotificacionesRepositorio
{
    public async Task<IReadOnlyList<NotificacionResponseDto>> ListarAsync(Guid destinatarioId, int limite, CancellationToken ct) =>
        await db.Notificacion.AsNoTracking()
            .Where(n => n.DestinatarioId == destinatarioId)
            .OrderByDescending(n => n.CreadaEn)
            .ThenByDescending(n => n.Id)
            .Take(limite)
            .Select(n => new NotificacionResponseDto(
                n.Id, n.Tipo, n.Titulo, n.Mensaje, n.CreadaEn, n.LeidaEn, n.TareaId, n.VersionDocumentoId))
            .ToListAsync(ct);

    public Task<int> ContarNoLeidasAsync(Guid destinatarioId, CancellationToken ct) =>
        ConsultasNotificacion.NoLeidas(db, destinatarioId).CountAsync(ct);
}
