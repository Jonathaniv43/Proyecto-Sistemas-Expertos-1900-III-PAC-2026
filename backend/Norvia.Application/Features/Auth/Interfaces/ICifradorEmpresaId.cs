namespace Norvia.Application.Features.Auth.Interfaces;

public interface ICifradorEmpresaId
{
    string Cifrar(Guid empresaId);
    bool TryDescifrar(string valor, out Guid empresaId);
}
