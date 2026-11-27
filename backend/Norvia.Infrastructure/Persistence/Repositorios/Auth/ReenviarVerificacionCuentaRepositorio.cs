using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.ReenviarVerificacionCuenta;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Auth;

public sealed class ReenviarVerificacionCuentaRepositorio(NorviaDbContext db) : IReenviarVerificacionCuentaRepositorio
{
    public Task<Acceso?> BuscarAccesoPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken) =>
        db.Acceso.FirstOrDefaultAsync(a => a.Email == emailNormalizado, cancellationToken);

    public Task<VerificacionCorreo?> BuscarVerificacionAsync(Guid accesoId, CancellationToken cancellationToken) =>
        db.VerificacionCorreo.FirstOrDefaultAsync(
            v => v.Proposito == PropositoVerificacionCorreo.ActivacionCuenta && v.AccesoId == accesoId, cancellationToken);

    public void AgregarVerificacion(VerificacionCorreo fila) => db.VerificacionCorreo.Add(fila);
}
