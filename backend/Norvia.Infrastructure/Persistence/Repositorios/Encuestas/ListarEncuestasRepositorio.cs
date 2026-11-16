using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Encuestas.ListarEncuestas;
using Norvia.Application.Features.Encuestas.ListarEncuestas.DTOs;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Encuestas;

public sealed class ListarEncuestasRepositorio(NorviaDbContext db) : IListarEncuestasRepositorio
{
    public async Task<IReadOnlyList<EncuestaResumenResponseDto>> ListarAsync(CancellationToken ct) =>
        await db.Encuesta.AsNoTracking()
            .OrderByDescending(e => e.CreadaEn)
            .Select(e => new EncuestaResumenResponseDto(
                e.Id,
                e.Titulo,
                e.Estado,
                e.CreadaEn,
                e.Versiones
                    .Where(v => v.Estado == EstadoVersionEncuesta.Publicada)
                    .Select(v => (int?)v.NumeroVersion)
                    .FirstOrDefault(),
                0))
            .ToListAsync(ct);
}
