namespace Norvia.Application.Common.Interfaces;

/// <summary>Cifra y descifra el EmpresaId que viaja al cliente y en el JWT.</summary>
public interface ICifradorEmpresaId
{
    string Cifrar(Guid empresaId);
    bool TryDescifrar(string valor, out Guid empresaId);
}
