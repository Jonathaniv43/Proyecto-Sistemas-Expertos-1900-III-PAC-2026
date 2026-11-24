using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Eventos.DTOs;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Eventos;

/// <summary>
/// Compartido por los repositorios de Eventos que devuelven el detalle. Accidente, InvestigacionAccidente, Tarea y
/// VinculoEmpresa son tablas de empresa: el filtro global limita a la sesión.
/// </summary>
internal static class ProyeccionEvento
{
    internal static async Task<EventoDetalleResponseDto?> DetalleAsync(NorviaDbContext db, Guid id, CancellationToken ct)
    {
        var f = await db.Accidente.AsNoTracking().Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id, a.Tipo, a.Estado, a.EstadoEnfermedad, a.EsGrave, a.FechaHoraEvento, a.Lugar, a.Descripcion, a.RegistradoEn,
                Investigacion = a.Investigacion == null ? null : new
                {
                    a.Investigacion.CausasInmediatas, a.Investigacion.CausasBasicas, a.Investigacion.Conclusiones,
                    a.Investigacion.InvestigadaPorId, a.Investigacion.InvestigadaPor.Persona, a.Investigacion.FechaInvestigacion
                },
                a.CerradoEn, a.DescartadoEn, DescartadoPor = a.DescartadoPor != null ? a.DescartadoPor.Persona : null, a.MotivoDescarte,
                Medidas = db.Tarea.Count(t => t.AccidenteId == a.Id && t.Estado != EstadoTarea.Cancelada)
            })
            .FirstOrDefaultAsync(ct);
        if (f is null) return null;

        var investigacion = f.Investigacion is { } i
            ? new InvestigacionResponseDto(i.CausasInmediatas, i.CausasBasicas, i.Conclusiones, i.InvestigadaPorId, i.Persona.NombreCompleto, i.FechaInvestigacion)
            : null;
        return new EventoDetalleResponseDto(f.Id, f.Tipo, f.Estado, f.EstadoEnfermedad, f.EsGrave, f.FechaHoraEvento, f.Lugar, f.Descripcion,
            f.RegistradoEn, investigacion, f.CerradoEn, f.DescartadoEn, f.DescartadoPor?.NombreCompleto, f.MotivoDescarte, f.Medidas);
    }
}
