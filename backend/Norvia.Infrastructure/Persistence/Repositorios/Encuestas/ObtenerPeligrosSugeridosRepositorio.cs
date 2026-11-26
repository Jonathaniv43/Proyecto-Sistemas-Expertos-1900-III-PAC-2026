using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Encuestas.ObtenerPeligrosSugeridos;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Encuestas;

// Todas las tablas de encuesta, respuesta y riesgo son de empresa: el filtro global limita a la empresa de la sesión.
public sealed class ObtenerPeligrosSugeridosRepositorio(NorviaDbContext db) : IObtenerPeligrosSugeridosRepositorio
{
    public Task<bool> ExisteEncuestaAsync(Guid encuestaId, CancellationToken ct) =>
        db.Encuesta.AnyAsync(e => e.Id == encuestaId, ct);

    public Task<int> ContarRespuestasAsync(Guid encuestaId, CancellationToken ct) =>
        db.RespuestaEncuesta.CountAsync(r => r.EncuestaVersion.EncuestaId == encuestaId, ct);

    public async Task<List<ExposicionRespondida>> ListarExposicionesAsync(Guid encuestaId, CancellationToken ct)
    {
        // Selección única: la opción elegida está en el detalle.
        var unica = await db.DetalleRespuesta.AsNoTracking()
            .Where(d => d.RespuestaEncuesta.EncuestaVersion.EncuestaId == encuestaId
                        && d.EncuestaOpcion != null && d.EncuestaOpcion.IndicaExposicion
                        && d.EncuestaPregunta.PeligroId != null)
            .Select(d => new ExposicionRespondida(
                d.RespuestaEncuestaId, d.EncuestaPreguntaId, d.EncuestaPregunta.Texto, d.EncuestaPregunta.PeligroId!.Value))
            .ToListAsync(ct);

        // Selección múltiple: las opciones elegidas van en DetalleRespuestaOpcion.
        var multiple = await db.DetalleRespuestaOpcion.AsNoTracking()
            .Where(o => o.DetalleRespuesta.RespuestaEncuesta.EncuestaVersion.EncuestaId == encuestaId
                        && o.EncuestaOpcion.IndicaExposicion
                        && o.DetalleRespuesta.EncuestaPregunta.PeligroId != null)
            .Select(o => new ExposicionRespondida(
                o.RespuestaEncuestaId, o.EncuestaPreguntaId, o.DetalleRespuesta.EncuestaPregunta.Texto,
                o.DetalleRespuesta.EncuestaPregunta.PeligroId!.Value))
            .ToListAsync(ct);

        return [.. unica, .. multiple];
    }

    // Peligro es un catálogo global.
    public Task<Dictionary<Guid, string>> NombresDePeligrosAsync(IReadOnlyCollection<Guid> peligroIds, CancellationToken ct) =>
        db.Peligro.AsNoTracking().Where(p => peligroIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Nombre, ct);

    public Task<List<RiesgoEmpresa>> ListarRiesgosDeEncuestaAsync(Guid encuestaId, CancellationToken ct) =>
        db.RiesgoEmpresa.Where(r => r.EncuestaId == encuestaId).ToListAsync(ct);

    public void AgregarRiesgo(RiesgoEmpresa riesgo) => db.RiesgoEmpresa.Add(riesgo);
}
