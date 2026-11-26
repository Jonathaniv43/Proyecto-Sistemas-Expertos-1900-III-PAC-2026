using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Catalogos.DTOs;
using Norvia.Application.Features.Catalogos.CambiarEstadoPeligro.DTOs;

namespace Norvia.Application.Features.Catalogos.CambiarEstadoPeligro;

/// <summary>Activa o desactiva. Un peligro inactivo no se puede asociar a preguntas nuevas; los riesgos y encuestas existentes lo conservan.</summary>
public sealed class CambiarEstadoPeligroServicio(ICambiarEstadoPeligroRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo) : IServicioCasoDeUso
{
    public async Task<Resultado<PeligroResponseDto>> EjecutarAsync(Guid id, CambiarEstadoPeligroRequestDto request, CancellationToken ct)
    {
        var elemento = await repositorio.BuscarAsync(id, ct);
        if (elemento is null) return Errores.PeligroNoEncontrado;

        elemento.Activo = request.Activo!.Value; // el validador exige el valor
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return PeligroResponseDto.De(elemento);
    }
}
