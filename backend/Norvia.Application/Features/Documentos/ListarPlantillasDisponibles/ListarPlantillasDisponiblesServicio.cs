using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Documentos.ListarPlantillasDisponibles;

/// <summary>
/// HU-070: las plantillas que la empresa puede usar, activas y de su rubro o de todos los rubros. Sin rubro en el
/// perfil no hay cuáles mostrar (409 <c>documentos.sin_rubro</c>).
/// </summary>
public sealed class ListarPlantillasDisponiblesServicio(IListarPlantillasDisponiblesRepositorio repositorio, ITenantActual sesion)
    : IServicioCasoDeUso
{
    public async Task<Resultado<IReadOnlyList<PlantillaDisponibleResponseDto>>> EjecutarAsync(CancellationToken ct)
    {
        if (sesion.EmpresaId is not Guid empresaId || sesion.VinculoEmpresaId is null) return Errores.SinVinculo;
        if (await repositorio.RubroDeEmpresaAsync(empresaId, ct) is not short rubroId) return Errores.SinRubro;
        return Resultado<IReadOnlyList<PlantillaDisponibleResponseDto>>.Exito(await repositorio.ListarAsync(rubroId, ct));
    }
}
