using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Empresas.ListarEmpresasPlataforma;
using Norvia.Application.Features.Empresas.ListarEmpresasPlataforma.DTOs;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Empresas;

public sealed class ListarEmpresasPlataformaRepositorio(NorviaDbContext db) : IListarEmpresasPlataformaRepositorio
{
    public async Task<IReadOnlyList<EmpresaResumenResponseDto>> ListarAsync(string? buscar, CancellationToken ct)
    {
        // Operación de plataforma (HU-024): sin EmpresaId en sesión, VinculoEmpresa devolvería 0 por HasQueryFilter.
        // Solo se cuentan vínculos activos agregados por empresa; no se consulta ningún dato SST ni personas individuales.
        var q = db.Empresa.AsNoTracking().IgnoreQueryFilters();

        if (buscar is not null)
        {
            var t = buscar.ToLower();
            q = q.Where(e => e.RazonSocial.ToLower().Contains(t)
                || (e.Rubro != null && e.Rubro.Nombre.ToLower().Contains(t)));
        }

        return await q
            .OrderByDescending(e => e.CreadaEn)
            .ThenBy(e => e.RazonSocial)
            .Select(e => new EmpresaResumenResponseDto(
                e.Id,
                e.RazonSocial,
                e.Rubro != null ? e.Rubro.Nombre : null,
                e.Estado,
                e.CreadaEn,
                e.Vinculos.Count(v => v.Estado == EstadoVinculo.Activo)))
            .ToListAsync(ct);
    }
}
