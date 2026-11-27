using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.VerificarCuenta;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Auth;

public sealed class VerificarCuentaRepositorio(NorviaDbContext db) : IVerificarCuentaRepositorio
{
    public Task<Acceso?> BuscarAccesoPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken) =>
        db.Acceso.FirstOrDefaultAsync(a => a.Email == emailNormalizado, cancellationToken);

    public Task<VerificacionCorreo?> BuscarVerificacionAsync(Guid accesoId, CancellationToken cancellationToken) =>
        db.VerificacionCorreo.FirstOrDefaultAsync(
            v => v.Proposito == PropositoVerificacionCorreo.ActivacionCuenta && v.AccesoId == accesoId, cancellationToken);

    public void EliminarVerificacion(VerificacionCorreo fila) => db.VerificacionCorreo.Remove(fila);
}
