using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Encuestas.DTOs;
using Norvia.Application.Features.Encuestas.ListarHistorialEncuestas;
using Norvia.Application.Features.Encuestas.ListarHistorialEncuestas.DTOs;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Encuestas;

public sealed class ListarHistorialEncuestasRepositorio(NorviaDbContext db) : IListarHistorialEncuestasRepositorio
{
    public async Task<IReadOnlyList<EncuestaHistorialResponseDto>> ListarAsync(
        FiltrosHistorialEncuestasDto filtros, CancellationToken ct)
    {
        var texto = filtros.Buscar?.Trim();

        var consulta = db.Encuesta.AsNoTracking()
            .Where(e => filtros.IncluirVigentes
                ? e.Versiones.Any(v => v.Estado != EstadoVersionEncuesta.Borrador)
                : e.Estado == EstadoEncuesta.Cerrada || e.Versiones.Any(v => v.Estado == EstadoVersionEncuesta.Reemplazada));

        if (!string.IsNullOrEmpty(texto))
        {
            var patron = $"%{EscaparLike(texto)}%";
            consulta = consulta.Where(e =>
                EF.Functions.ILike(e.Titulo, patron, "\\")
                || e.Versiones.Any(v =>
                    v.Estado != EstadoVersionEncuesta.Borrador
                    && v.Categorias.Any(c => EF.Functions.ILike(c.CategoriaRiesgo.Nombre, patron, "\\"))));
        }

        var filas = await consulta
            .Select(e => new
            {
                e.Id,
                e.Titulo,
                e.Estado,
                e.CreadaEn,
                e.CerradaEn,
                Participaciones = db.ParticipacionEncuesta.Count(p => p.EncuestaId == e.Id),
                Versiones = e.Versiones
                    .Where(v => v.Estado != EstadoVersionEncuesta.Borrador)
                    .OrderByDescending(v => v.NumeroVersion)
                    .Select(v => new
                    {
                        v.Id,
                        v.NumeroVersion,
                        v.Estado,
                        v.PublicadaEn,
                        Respuestas = db.RespuestaEncuesta.Count(r => r.EncuestaVersionId == v.Id),
                        Categorias = v.Categorias.Select(c => c.CategoriaRiesgo.Nombre)
                    })
                    .ToList()
            })
            .ToListAsync(ct);

        return filas
            .Select(e =>
            {
                var versiones = e.Versiones
                    .Select(v => new VersionEncuestaResumenResponseDto(
                        v.Id,
                        v.NumeroVersion,
                        v.Estado,
                        v.PublicadaEn,
                        v.Respuestas,
                        v.Categorias.Distinct().OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList()))
                    .ToList();

                var categorias = versiones
                    .SelectMany(v => v.Categorias)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var ultimaPublicacion = versiones.Max(v => v.PublicadaEn);
                var respuestasAnonimas = versiones.Sum(v => v.TotalRespuestasAnonimas);

                return new EncuestaHistorialResponseDto(
                    e.Id,
                    e.Titulo,
                    e.Estado,
                    e.CreadaEn,
                    e.CerradaEn,
                    ultimaPublicacion,
                    categorias,
                    new ResumenHistorialEncuestaDto(e.Participaciones, respuestasAnonimas),
                    versiones);
            })
            .OrderByDescending(e => e.CerradaEn ?? e.UltimaPublicacionEn ?? e.CreadaEn)
            .ThenByDescending(e => e.CreadaEn)
            .ToList();
    }

    private static string EscaparLike(string valor) =>
        valor.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
