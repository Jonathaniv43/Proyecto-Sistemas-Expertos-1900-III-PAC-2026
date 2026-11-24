using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Riesgos.DTOs;
using Norvia.Application.Features.Riesgos.ListarRiesgos;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Riesgos;

// RiesgoEmpresa y Encuesta son tablas de empresa: el filtro global limita a la empresa de la sesión.
public sealed class ListarRiesgosRepositorio(NorviaDbContext db) : IListarRiesgosRepositorio
{
    public async Task<IReadOnlyList<RiesgoResponseDto>> ListarConfirmadosAsync(CancellationToken ct) =>
        await ProyeccionRiesgo.Proyectar(db, db.RiesgoEmpresa.AsNoTracking()
                .Where(r => r.Estado == EstadoRiesgo.Confirmado)
                .OrderByDescending(r => r.CreadoEn))
            .ToListAsync(ct);
}
