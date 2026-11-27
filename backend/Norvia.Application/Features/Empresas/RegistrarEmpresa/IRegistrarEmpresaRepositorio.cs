using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Empresas.RegistrarEmpresa;

public interface IRegistrarEmpresaRepositorio
{
    Task<bool> ExisteRtnAsync(string rtn, CancellationToken cancellationToken);
    Task<bool> ExisteEmailAsync(string emailNormalizado, CancellationToken cancellationToken);
    Task<bool> ExisteDocumentoAsync(TipoDocumentoIdentidad tipoDocumento, string numeroDocumento, CancellationToken cancellationToken);
    Task<bool> TipoDocumentoIdentidadExisteAsync(string codigo, CancellationToken cancellationToken);
    Task<short?> BuscarRubroIdActivoAsync(string codigo, CancellationToken cancellationToken);
    void Agregar(Empresa empresa, Persona persona, Usuario usuario, Acceso acceso, VinculoEmpresa vinculo, VerificacionCorreo verificacion);
}