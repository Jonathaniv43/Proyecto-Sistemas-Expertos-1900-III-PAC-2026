using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Encuestas.DTOs;
using Norvia.Application.Features.Encuestas.ObtenerEncuesta;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Encuestas;

public sealed class ObtenerEncuestaRepositorio(NorviaDbContext db) : IObtenerEncuestaRepositorio
{
    public Task<Encuesta?> BuscarAsync(Guid id, CancellationToken ct) =>
        db.Encuesta.AsNoTracking()
            .Include(e => e.Versiones.Where(v =>
                v.Estado == EstadoVersionEncuesta.Publicada || v.Estado == EstadoVersionEncuesta.Borrador))
                .ThenInclude(v => v.Secciones)
            .Include(e => e.Versiones.Where(v =>
                v.Estado == EstadoVersionEncuesta.Publicada || v.Estado == EstadoVersionEncuesta.Borrador))
                .ThenInclude(v => v.Preguntas)
                    .ThenInclude(p => p.Opciones)
            .Include(e => e.Versiones.Where(v =>
                v.Estado == EstadoVersionEncuesta.Publicada || v.Estado == EstadoVersionEncuesta.Borrador))
                .ThenInclude(v => v.Preguntas)
                    .ThenInclude(p => p.TipoRespuestaPregunta)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<VersionEncuestaResumenResponseDto>> ListarVersionesPublicadasAsync(
        Guid encuestaId, CancellationToken ct)
    {
        var filas = await db.EncuestaVersion.AsNoTracking()
            .Where(v => v.EncuestaId == encuestaId && v.Estado != EstadoVersionEncuesta.Borrador)
            .OrderByDescending(v => v.NumeroVersion)
            .Select(v => new
            {
                v.Id,
                v.NumeroVersion,
                v.Estado,
                v.PublicadaEn,
                TotalRespuestasAnonimas = db.RespuestaEncuesta.Count(r => r.EncuestaVersionId == v.Id),
                Categorias = v.Categorias.Select(c => c.CategoriaRiesgo.Nombre)
            })
            .ToListAsync(ct);

        return filas
            .Select(v => new VersionEncuestaResumenResponseDto(
                v.Id,
                v.NumeroVersion,
                v.Estado,
                v.PublicadaEn,
                v.TotalRespuestasAnonimas,
                v.Categorias.Distinct().OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList()))
            .ToList();
    }
}
