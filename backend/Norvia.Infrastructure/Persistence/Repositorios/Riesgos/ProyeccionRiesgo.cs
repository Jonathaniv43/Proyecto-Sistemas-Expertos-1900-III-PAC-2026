using Norvia.Application.Features.Riesgos.DTOs;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Riesgos;

/// <summary>Compartido por los repositorios del módulo Riesgos que devuelven <see cref="RiesgoResponseDto"/>.</summary>
internal static class ProyeccionRiesgo
{
    // Proyección común: nombre del peligro (catálogo global) y título de la encuesta que lo sugirió (misma empresa).
    internal static IQueryable<RiesgoResponseDto> Proyectar(NorviaDbContext db, IQueryable<RiesgoEmpresa> riesgos) =>
        riesgos.Select(r => new RiesgoResponseDto(
            r.Id, r.Estado, r.PeligroId, r.Peligro != null ? r.Peligro.Nombre : null, r.DescripcionPeligro, r.Actividad,
            r.EncuestaId, db.Encuesta.Where(e => e.Id == r.EncuestaId).Select(e => e.Titulo).FirstOrDefault(), r.CreadoEn));
}
