using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Empresas;

namespace Norvia.Application.Features.Empresas.RegistrarEmpresa;

public interface IRegistrarEmpresaRepositorio
{
    Task<bool> ExisteRtnAsync(string rtn, CancellationToken cancellationToken);
    Task<bool> ExisteEmailAsync(string emailNormalizado, CancellationToken cancellationToken);
    void Agregar(Empresa empresa, Persona persona, Usuario usuario, Acceso acceso, VinculoEmpresa vinculo);
}