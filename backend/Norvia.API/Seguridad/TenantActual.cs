using Norvia.Application.Common.Interfaces;
using Sesion = Norvia.Domain.Enums.TipoSesion;

namespace Norvia.API.Seguridad;

/// <summary>
/// Lee el tenant de los claims del JWT ya validado. Solo una sesión de tipo <c>empresa</c>
/// con <c>empresa_id</c> válido (cifrado) produce un EmpresaId. Se calcula una vez por petición.
/// </summary>
public sealed class TenantActual : ITenantActual
{
    private readonly Lazy<(Guid? Empresa, Guid? Vinculo, Sesion? Tipo)> _datos;

    public TenantActual(IHttpContextAccessor accesor, ICifradorEmpresaId cifrador)
    {
        _datos = new(() =>
        {
            var usuario = accesor.HttpContext?.User;
            if (usuario?.Identity?.IsAuthenticated != true)
                return (null, null, null);

            Sesion? tipo = usuario.FindFirst("tipo_sesion")?.Value switch
            {
                "empresa" => Sesion.Empresa,
                "plataforma" => Sesion.Plataforma,
                _ => null
            };
            if (tipo != Sesion.Empresa)
                return (null, null, tipo);

            var empresaCifrada = usuario.FindFirst("empresa_id")?.Value;
            Guid? empresa = empresaCifrada is not null && cifrador.TryDescifrar(empresaCifrada, out var id) ? id : null;
            Guid? vinculo = Guid.TryParse(usuario.FindFirst("vinculo_id")?.Value, out var v) ? v : null;

            // Un token de empresa sin empresa o sin vínculo válidos no da acceso a ningún tenant.
            return empresa is null || vinculo is null ? (null, null, tipo) : (empresa, vinculo, tipo);
        });
    }

    public Guid? EmpresaId => _datos.Value.Empresa;
    public Guid? VinculoEmpresaId => _datos.Value.Vinculo;
    public Sesion? TipoSesion => _datos.Value.Tipo;
}
