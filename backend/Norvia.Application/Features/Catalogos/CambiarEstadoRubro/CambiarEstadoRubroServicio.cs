using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Catalogos.DTOs;
using Norvia.Application.Features.Catalogos.CambiarEstadoRubro.DTOs;

namespace Norvia.Application.Features.Catalogos.CambiarEstadoRubro;

/// <summary>Activa o desactiva. Un rubro inactivo no se ofrece a las empresas nuevas ni a categorías o plantillas nuevas; las que ya lo tienen lo conservan.</summary>
public sealed class CambiarEstadoRubroServicio(ICambiarEstadoRubroRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo) : IServicioCasoDeUso
{
    public async Task<Resultado<RubroResponseDto>> EjecutarAsync(short id, CambiarEstadoRubroRequestDto request, CancellationToken ct)
    {
        var elemento = await repositorio.BuscarAsync(id, ct);
        if (elemento is null) return Errores.RubroNoEncontrado;

        elemento.Activo = request.Activo!.Value; // el validador exige el valor
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return RubroResponseDto.De(elemento);
    }
}
