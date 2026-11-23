using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Empresas.ListarRubrosRegistro.DTOs;

namespace Norvia.Application.Features.Empresas.ListarRubrosRegistro;

/// <summary>
/// HU-017: rubros que se pueden elegir al registrar una empresa. Es anónimo porque quien se registra
/// todavía no tiene sesión; por eso solo expone código y nombre de los rubros activos.
/// El mantenimiento completo del catálogo sigue en GET /api/rubros (staff, HU-023).
/// </summary>
public sealed class ListarRubrosRegistroServicio(IListarRubrosRegistroRepositorio repositorio) : IServicioCasoDeUso
{
    public Task<List<RubroRegistroResponseDto>> EjecutarAsync(CancellationToken cancellationToken) =>
        repositorio.ListarActivosAsync(cancellationToken);
}
