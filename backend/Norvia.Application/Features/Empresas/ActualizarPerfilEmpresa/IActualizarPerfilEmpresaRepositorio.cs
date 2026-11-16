using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;

namespace Norvia.Application.Features.Empresas.ActualizarPerfilEmpresa;

public interface IActualizarPerfilEmpresaRepositorio
{
    /// <summary>Empresa tracked por Id (tabla raíz sin filtro global). Incluye Rubro.</summary>
    Task<Empresa?> BuscarAsync(Guid empresaId, CancellationToken ct);

    /// <summary>Id del rubro activo con ese código, o null si no existe / no está activo.</summary>
    Task<short?> BuscarRubroIdActivoAsync(string codigo, CancellationToken ct);

    /// <summary>Carga la navegación Rubro tras cambiar RubroId (para el response).</summary>
    Task<Rubro?> BuscarRubroAsync(short rubroId, CancellationToken ct);
}
