using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Auth.Interfaces;

public interface IVerificadorPassword
{
    bool Verificar(Acceso acceso, string password);
}
