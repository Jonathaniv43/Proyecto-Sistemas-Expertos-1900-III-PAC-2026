using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Eventos.ListarEventos;
using Norvia.Application.Features.Eventos.ListarEventos.DTOs;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Eventos;

public sealed class ListarEventosRepositorio(NorviaDbContext db) : IListarEventosRepositorio
{
    private const int MaxResumenDescripcion = 200;

    public async Task<IReadOnlyList<EventoResumenResponseDto>> ListarAsync(
        EstadoAccidente? estado, TipoAccidente? tipo, bool? esGrave, CancellationToken ct)
    {
        var query = db.Accidente.AsNoTracking();

        if (estado.HasValue)
            query = query.Where(a => a.Estado == estado.Value);

        if (tipo.HasValue)
            query = query.Where(a => a.Tipo == tipo.Value);

        if (esGrave.HasValue)
            query = query.Where(a => a.EsGrave == esGrave.Value);

        var filas = await query
            .OrderByDescending(a => a.RegistradoEn)
            .ThenByDescending(a => a.FechaHoraEvento)
            .Select(a => new
            {
                a.Id,
                a.Tipo,
                a.Estado,
                a.EstadoEnfermedad,
                a.EsGrave,
                a.FechaHoraEvento,
                a.Lugar,
                a.Descripcion,
                a.RegistradoEn
            })
            .ToListAsync(ct);

        return filas.Select(a => new EventoResumenResponseDto(
            a.Id,
            a.Tipo,
            a.Estado,
            a.EstadoEnfermedad,
            a.EsGrave,
            a.FechaHoraEvento,
            a.Lugar,
            AcortarDescripcion(a.Descripcion),
            a.RegistradoEn)).ToList();
    }

    private static string AcortarDescripcion(string descripcion) =>
        descripcion.Length <= MaxResumenDescripcion
            ? descripcion
            : string.Concat(descripcion.AsSpan(0, MaxResumenDescripcion - 3).TrimEnd(), "...");
}
