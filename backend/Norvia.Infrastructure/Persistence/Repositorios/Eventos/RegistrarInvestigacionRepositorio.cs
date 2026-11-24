using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Eventos;
using Norvia.Application.Features.Eventos.DTOs;
using Norvia.Application.Features.Eventos.RegistrarInvestigacion;
using Norvia.Domain.Entities.Accidentes;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Eventos;

public sealed class RegistrarInvestigacionRepositorio(NorviaDbContext db) : IRegistrarInvestigacionRepositorio
{
    public Task<Accidente?> BuscarAsync(Guid id, CancellationToken ct) =>
        db.Accidente.Include(a => a.Investigacion).FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<bool> EsInvestigadorValidoAsync(Guid vinculoId, CancellationToken ct) =>
        db.VinculoEmpresa.AnyAsync(v => v.Id == vinculoId && v.Estado == EstadoVinculo.Activo && Reglas.RolesInvestigadores.Contains(v.RolId), ct);

    public void Agregar(InvestigacionAccidente investigacion) => db.InvestigacionAccidente.Add(investigacion);

    public async Task<EventoDetalleResponseDto> DetalleAsync(Guid id, CancellationToken ct) => (await ProyeccionEvento.DetalleAsync(db, id, ct))!;
}
