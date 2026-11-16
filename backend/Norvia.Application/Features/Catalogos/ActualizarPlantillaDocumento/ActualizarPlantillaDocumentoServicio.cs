using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Catalogos.DTOs;
using Norvia.Application.Features.Catalogos.ActualizarPlantillaDocumento.DTOs;

namespace Norvia.Application.Features.Catalogos.ActualizarPlantillaDocumento;

/// <summary>Edita una plantilla de documento. Los documentos ya creados por las empresas no cambian.</summary>
public sealed class ActualizarPlantillaDocumentoServicio(
    IActualizarPlantillaDocumentoRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo) : IServicioCasoDeUso
{
    public async Task<Resultado<PlantillaDocumentoResponseDto>> EjecutarAsync(
        Guid id, ActualizarPlantillaDocumentoRequestDto request, CancellationToken ct)
    {
        var plantilla = await repositorio.BuscarAsync(id, ct);
        if (plantilla is null) return Errores.PlantillaNoEncontrada;

        if (request.RubroId is short pedido)
        {
            short? rubroId = pedido == 0 ? null : pedido;
            if (rubroId is short r && r != plantilla.RubroId && !await repositorio.RubroActivoExisteAsync(r, ct))
                return Errores.RubroInvalido;
            plantilla.RubroId = rubroId;
        }
        if (request.Titulo is not null) plantilla.Titulo = request.Titulo.Trim();
        if (request.TipoDocumento is { } tipo) plantilla.TipoDocumento = tipo;
        if (request.Contenido is not null) plantilla.Contenido = request.Contenido;
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return PlantillaDocumentoResponseDto.De(plantilla);
    }
}
