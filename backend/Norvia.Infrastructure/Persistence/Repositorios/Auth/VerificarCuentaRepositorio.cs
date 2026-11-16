using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.VerificarCuenta;
using Norvia.Domain.Entities.Acceso;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Auth;

public sealed class VerificarCuentaRepositorio(NorviaDbContext db) : IVerificarCuentaRepositorio
{
    public Task<Acceso?> BuscarAccesoPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken) =>
        db.Acceso.FirstOrDefaultAsync(a => a.Email == emailNormalizado, cancellationToken);
}
