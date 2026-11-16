using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.ReenviarVerificacionCuenta;
using Norvia.Domain.Entities.Acceso;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Auth;

public sealed class ReenviarVerificacionCuentaRepositorio(NorviaDbContext db) : IReenviarVerificacionCuentaRepositorio
{
    public Task<Acceso?> BuscarAccesoPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken) =>
        db.Acceso.FirstOrDefaultAsync(a => a.Email == emailNormalizado, cancellationToken);
}
