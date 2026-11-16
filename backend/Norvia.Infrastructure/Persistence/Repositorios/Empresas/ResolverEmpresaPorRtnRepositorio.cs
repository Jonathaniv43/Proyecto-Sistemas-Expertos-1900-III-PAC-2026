using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Empresas.ResolverEmpresaPorRtn;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Empresas;

public sealed class ResolverEmpresaPorRtnRepositorio(NorviaDbContext db) : IResolverEmpresaPorRtnRepositorio
{
    public Task<EmpresaPorRtnLectura?> BuscarActivaPorRtnAsync(string rtn, CancellationToken cancellationToken) =>
        db.Empresa
            .Where(e => e.RTN == rtn && e.Estado == EstadoEmpresa.Activa)
            .Select(e => new EmpresaPorRtnLectura(e.Id, e.RazonSocial, e.RTN))
            .FirstOrDefaultAsync(cancellationToken);
}