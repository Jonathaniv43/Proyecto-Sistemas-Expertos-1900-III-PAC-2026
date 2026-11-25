using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.VerificarCodigoMicrosoft;
using Norvia.Domain.Entities.Acceso;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Auth;

public sealed class VerificarCodigoMicrosoftRepositorio(NorviaDbContext db) : IVerificarCodigoMicrosoftRepositorio
{
    public Task<VerificacionCorreoMicrosoft?> BuscarAsync(string email, string proveedorUserId, CancellationToken cancellationToken) =>
        db.VerificacionCorreoMicrosoft.FirstOrDefaultAsync(v => v.Email == email && v.ProveedorUserId == proveedorUserId, cancellationToken);

    public void Eliminar(VerificacionCorreoMicrosoft fila) => db.VerificacionCorreoMicrosoft.Remove(fila);
}
