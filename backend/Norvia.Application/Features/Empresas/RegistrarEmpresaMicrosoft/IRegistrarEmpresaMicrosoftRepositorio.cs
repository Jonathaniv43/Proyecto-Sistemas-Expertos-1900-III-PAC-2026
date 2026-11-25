using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Empresas.RegistrarEmpresaMicrosoft;

public interface IRegistrarEmpresaMicrosoftRepositorio
{
    Task<bool> ExisteRtnAsync(string rtn, CancellationToken cancellationToken);
    Task<bool> ExisteEmailAsync(string emailNormalizado, CancellationToken cancellationToken);
    Task<bool> ExisteDocumentoAsync(TipoDocumentoIdentidad tipoDocumento, string numeroDocumento, CancellationToken cancellationToken);
    Task<short?> BuscarRubroIdActivoAsync(string codigo, CancellationToken cancellationToken);

    /// <summary>Acceso con esa cuenta del proveedor, o null.</summary>
    Task<Acceso?> BuscarAccesoPorProveedorAsync(ProveedorIdentidad proveedor, string proveedorUserId, CancellationToken cancellationToken);

    /// <summary>Persona del Acceso, rastreada para poder completar sus datos vacíos (RN-85), o null.</summary>
    Task<Persona?> BuscarPersonaDeAccesoAsync(Guid accesoId, CancellationToken cancellationToken);

    Task<bool> UsuarioActivoAsync(Guid personaId, CancellationToken cancellationToken);

    /// <summary>Persona, cuenta y proveedor nuevos. Deja todo pendiente; no guarda.</summary>
    void AgregarTodo(Empresa empresa, Persona persona, Usuario usuario, Acceso acceso, AccesoProveedor proveedor, VinculoEmpresa vinculo);

    /// <summary>Persona y cuenta que ya existen: solo la empresa y su vínculo. Deja todo pendiente; no guarda.</summary>
    void AgregarEmpresaYVinculo(Empresa empresa, VinculoEmpresa vinculo);
}
