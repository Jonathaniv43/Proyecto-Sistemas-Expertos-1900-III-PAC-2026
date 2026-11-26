using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Encuestas.ListarCategoriasDisponibles.DTOs;

namespace Norvia.Application.Features.Encuestas.ListarCategoriasDisponibles;

/// <summary>
/// HU-029: categorías de riesgo que el Responsable SST puede elegir para una encuesta, según el rubro
/// de la empresa de la sesión (RN-09, RN-11), con la vista previa de las preguntas que incluiría cada una.
/// </summary>
public sealed class ListarCategoriasDisponiblesServicio(
    IListarCategoriasDisponiblesRepositorio repositorio, ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado<IReadOnlyList<CategoriaDisponibleResponseDto>>> EjecutarAsync(CancellationToken ct)
    {
        if (sesion.EmpresaId is not Guid empresaId) return Errores.EmpresaSinRubro;

        var rubroId = await repositorio.BuscarRubroDeEmpresaAsync(empresaId, ct);
        if (rubroId is not short rubro) return Errores.EmpresaSinRubro;

        var categorias = await repositorio.ListarActivasDelRubroAsync(rubro, ct);
        IReadOnlyList<CategoriaDisponibleResponseDto> respuesta = categorias
            .Select(c => new CategoriaDisponibleResponseDto(
                c.Id, c.Nombre, c.Descripcion,
                c.Preguntas
                    .OrderBy(p => p.Orden)
                    .Select(p => new PreguntaPreviewDto(
                        p.Id, p.Texto,
                        p.TipoRespuestaPregunta?.Codigo ?? string.Empty,
                        p.TipoRespuestaPregunta?.InstruccionUi ?? string.Empty,
                        p.Obligatoria, p.Orden,
                        p.Opciones.OrderBy(o => o.Orden).Select(o => new OpcionPreviewDto(o.Texto, o.Orden)).ToList()))
                    .ToList()))
            .ToList();
        return Resultado<IReadOnlyList<CategoriaDisponibleResponseDto>>.Exito(respuesta);
    }
}
