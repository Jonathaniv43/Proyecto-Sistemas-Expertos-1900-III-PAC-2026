using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Catalogos.DTOs;
using Norvia.Application.Features.Catalogos.CambiarEstadoPregunta.DTOs;

namespace Norvia.Application.Features.Catalogos.CambiarEstadoPregunta;

/// <summary>Activa o desactiva. Una pregunta inactiva no se copia a encuestas nuevas; las ya generadas no cambian (RN-12).</summary>
public sealed class CambiarEstadoPreguntaServicio(ICambiarEstadoPreguntaRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo) : IServicioCasoDeUso
{
    public async Task<Resultado<PreguntaResponseDto>> EjecutarAsync(Guid id, CambiarEstadoPreguntaRequestDto request, CancellationToken ct)
    {
        var elemento = await repositorio.BuscarAsync(id, ct);
        if (elemento is null) return Errores.PreguntaNoEncontrada;

        elemento.Activa = request.Activo!.Value; // el validador exige el valor
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return PreguntaResponseDto.De(elemento);
    }
}
