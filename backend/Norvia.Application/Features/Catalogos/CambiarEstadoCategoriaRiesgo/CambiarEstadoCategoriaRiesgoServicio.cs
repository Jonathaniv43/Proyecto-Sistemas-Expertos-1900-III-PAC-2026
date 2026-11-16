using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Catalogos.DTOs;
using Norvia.Application.Features.Catalogos.CambiarEstadoCategoriaRiesgo.DTOs;

namespace Norvia.Application.Features.Catalogos.CambiarEstadoCategoriaRiesgo;

/// <summary>Activa o desactiva. Una categoría inactiva no se usa en encuestas nuevas ni recibe preguntas nuevas.</summary>
public sealed class CambiarEstadoCategoriaRiesgoServicio(ICambiarEstadoCategoriaRiesgoRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo) : IServicioCasoDeUso
{
    public async Task<Resultado<CategoriaRiesgoResponseDto>> EjecutarAsync(Guid id, CambiarEstadoCategoriaRiesgoRequestDto request, CancellationToken ct)
    {
        var elemento = await repositorio.BuscarAsync(id, ct);
        if (elemento is null) return Errores.CategoriaNoEncontrada;

        elemento.Activa = request.Activo!.Value; // el validador exige el valor
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return CategoriaRiesgoResponseDto.De(elemento);
    }
}
