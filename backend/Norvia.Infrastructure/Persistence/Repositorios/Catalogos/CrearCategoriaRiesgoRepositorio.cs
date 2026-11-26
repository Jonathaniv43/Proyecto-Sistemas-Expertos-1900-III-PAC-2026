using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.CrearCategoriaRiesgo;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tablas globales (sin EmpresaId): no les aplica el filtro por empresa.
public sealed class CrearCategoriaRiesgoRepositorio(NorviaDbContext db) : ICrearCategoriaRiesgoRepositorio
{
    public Task<bool> RubroActivoExisteAsync(short rubroId, CancellationToken ct) =>
        db.Rubro.AnyAsync(r => r.Id == rubroId && r.Activo, ct);

    public Task<bool> ExisteNombreAsync(short rubroId, string nombre, CancellationToken ct) =>
        db.CategoriaRiesgo.AnyAsync(c => c.RubroId == rubroId && c.Nombre == nombre, ct);

    public void Agregar(CategoriaRiesgo categoria) => db.CategoriaRiesgo.Add(categoria);
}
