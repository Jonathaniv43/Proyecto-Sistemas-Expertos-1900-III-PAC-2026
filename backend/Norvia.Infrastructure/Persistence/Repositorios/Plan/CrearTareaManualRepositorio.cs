using Norvia.Application.Features.Plan.CrearTareaManual;
using Norvia.Application.Features.Plan.DTOs;
using Norvia.Domain.Entities.Plan;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Plan;

// Tarea y VinculoEmpresa son tablas de empresa: el filtro global limita a la sesión y EmpresaId se asigna al guardar.
public sealed class CrearTareaManualRepositorio(NorviaDbContext db) : ICrearTareaManualRepositorio
{
    public Task<bool> EsResponsableValidoAsync(Guid vinculoId, CancellationToken ct) => ConsultaResponsables.EsValidoAsync(db, vinculoId, ct);

    public void Agregar(Tarea tarea) => db.Tarea.Add(tarea);

    public async Task<TareaResponseDto> ProyectarAsync(Guid tareaId, CancellationToken ct) =>
        (await ProyeccionTarea.ListarAsync(db.Tarea.Where(t => t.Id == tareaId), ct)).Single();
}
