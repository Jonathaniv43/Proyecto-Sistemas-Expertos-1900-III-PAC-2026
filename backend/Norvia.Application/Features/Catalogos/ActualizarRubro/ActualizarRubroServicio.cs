using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Catalogos.DTOs;
using Norvia.Application.Features.Catalogos.ActualizarRubro.DTOs;

namespace Norvia.Application.Features.Catalogos.ActualizarRubro;

/// <summary>Edita un rubro. Las empresas que ya lo tienen lo conservan.</summary>
public sealed class ActualizarRubroServicio(IActualizarRubroRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo)
    : IServicioCasoDeUso
{
    public async Task<Resultado<RubroResponseDto>> EjecutarAsync(
        short id, ActualizarRubroRequestDto request, CancellationToken ct)
    {
        var rubro = await repositorio.BuscarAsync(id, ct);
        if (rubro is null) return Errores.RubroNoEncontrado;

        if (request.Nombre is not null)
        {
            var nombre = request.Nombre.Trim();
            if (await repositorio.ExisteNombreEnOtroAsync(id, nombre, ct)) return Errores.NombreDuplicado;
            rubro.Nombre = nombre;
        }

        await unidadDeTrabajo.GuardarCambiosAsync(ct);
        return RubroResponseDto.De(rubro);
    }
}
