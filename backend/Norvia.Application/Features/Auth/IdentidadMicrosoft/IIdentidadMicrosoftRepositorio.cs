using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Auth.IdentidadMicrosoft;

public interface IIdentidadMicrosoftRepositorio
{
    /// <summary>Acceso del correo, o null. Solo importa si tiene contraseña (<c>PasswordHash</c>).</summary>
    Task<Acceso?> BuscarAccesoPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken);

    /// <summary>Acceso al que pertenece esa cuenta del proveedor, o null.</summary>
    Task<Acceso?> BuscarAccesoPorProveedorAsync(ProveedorIdentidad proveedor, string proveedorUserId, CancellationToken cancellationToken);

    /// <summary>Persona del Acceso (la de cualquiera de sus vínculos), o null si solo es staff.</summary>
    Task<Persona?> BuscarPersonaDeAccesoAsync(Guid accesoId, CancellationToken cancellationToken);
}
