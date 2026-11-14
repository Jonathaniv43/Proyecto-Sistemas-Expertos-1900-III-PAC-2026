using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Common.Interfaces;

/// <summary>Hash y verificación de contraseñas de <see cref="Acceso"/>.</summary>
public interface IHasherPassword
{
    string Hashear(Acceso acceso, string password);
    bool Verificar(Acceso acceso, string password);
}
