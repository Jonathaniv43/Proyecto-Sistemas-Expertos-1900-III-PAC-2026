using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.VerificarCodigoMicrosoft;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Auth;

public sealed class VerificarCodigoMicrosoftRepositorio(NorviaDbContext db) : IVerificarCodigoMicrosoftRepositorio
{
    public Task<VerificacionCorreo?> BuscarAsync(string email, string proveedorUserId, CancellationToken cancellationToken) =>
        db.VerificacionCorreo.FirstOrDefaultAsync(
            v => v.Proposito == PropositoVerificacionCorreo.Microsoft && v.Email == email && v.ProveedorUserId == proveedorUserId,
            cancellationToken);

    public void Eliminar(VerificacionCorreo fila) => db.VerificacionCorreo.Remove(fila);
}
