using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Catalogos.DTOs;
using Norvia.Application.Features.Catalogos.CrearPlantillaDocumento.DTOs;
using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.CrearPlantillaDocumento;

/// <summary>Crea una política o procedimiento modelo (D13).</summary>
public sealed class CrearPlantillaDocumentoServicio(
    ICrearPlantillaDocumentoRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo) : IServicioCasoDeUso
{
    public async Task<Resultado<PlantillaDocumentoResponseDto>> EjecutarAsync(
        CrearPlantillaDocumentoRequestDto request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim();
        if (await repositorio.ExisteCodigoAsync(codigo, ct)) return Errores.CodigoDuplicado;
        if (request.RubroId is short rubroId && !await repositorio.RubroActivoExisteAsync(rubroId, ct))
            return Errores.RubroInvalido;

        var plantilla = new PlantillaDocumento
        {
            Codigo = codigo,
            Titulo = request.Titulo.Trim(),
            TipoDocumento = request.TipoDocumento,
            RubroId = request.RubroId,
            Contenido = request.Contenido,
            Activa = true
        };
        repositorio.Agregar(plantilla);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return PlantillaDocumentoResponseDto.De(plantilla);
    }
}
