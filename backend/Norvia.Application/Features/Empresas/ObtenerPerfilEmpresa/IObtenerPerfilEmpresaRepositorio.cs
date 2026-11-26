using Norvia.Domain.Entities.Empresas;

namespace Norvia.Application.Features.Empresas.ObtenerPerfilEmpresa;

public interface IObtenerPerfilEmpresaRepositorio
{
    /// <summary>Empresa por Id (tabla raíz sin filtro global). Incluye Rubro.</summary>
    Task<Empresa?> BuscarAsync(Guid empresaId, CancellationToken ct);
}
