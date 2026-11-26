using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Eventos;
using Norvia.Application.Features.Eventos.CrearMedidaCorrectiva;
using Norvia.Application.Features.Eventos.DTOs;
using Norvia.Domain.Entities.Accidentes;
using Norvia.Domain.Entities.Plan;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Eventos;

// Accidente, Tarea y VinculoEmpresa son tablas de empresa: el filtro global limita a la sesión y EmpresaId se asigna al guardar.
public sealed class CrearMedidaCorrectivaRepositorio(NorviaDbContext db) : ICrearMedidaCorrectivaRepositorio
{
    public Task<Accidente?> BuscarAsync(Guid id, CancellationToken ct) =>
        db.Accidente.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<bool> EsResponsableValidoAsync(Guid vinculoId, CancellationToken ct) =>
        db.VinculoEmpresa.AnyAsync(v => v.Id == vinculoId && v.Estado == EstadoVinculo.Activo && Reglas.RolesGestion.Contains(v.RolId), ct);

    public void Agregar(Tarea tarea) => db.Tarea.Add(tarea);

    public async Task<MedidaCorrectivaResponseDto> ProyectarAsync(Guid tareaId, CancellationToken ct) =>
        (await ProyeccionMedida.ListarAsync(db.Tarea.Where(t => t.Id == tareaId), ct)).Single();
}
