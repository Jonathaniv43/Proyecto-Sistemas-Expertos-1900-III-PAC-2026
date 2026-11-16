using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Catalogos.DTOs;
using Norvia.Application.Features.Catalogos.CambiarEstadoPlantillaDocumento.DTOs;

namespace Norvia.Application.Features.Catalogos.CambiarEstadoPlantillaDocumento;

/// <summary>Activa o desactiva. Una plantilla inactiva no se ofrece para documentos nuevos.</summary>
public sealed class CambiarEstadoPlantillaDocumentoServicio(ICambiarEstadoPlantillaDocumentoRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo) : IServicioCasoDeUso
{
    public async Task<Resultado<PlantillaDocumentoResponseDto>> EjecutarAsync(Guid id, CambiarEstadoPlantillaDocumentoRequestDto request, CancellationToken ct)
    {
        var elemento = await repositorio.BuscarAsync(id, ct);
        if (elemento is null) return Errores.PlantillaNoEncontrada;

        elemento.Activa = request.Activo!.Value; // el validador exige el valor
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return PlantillaDocumentoResponseDto.De(elemento);
    }
}
