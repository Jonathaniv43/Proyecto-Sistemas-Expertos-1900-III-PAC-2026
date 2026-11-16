using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Infrastructure.Persistence.Seed;

/// <summary>Filas iniciales; <see cref="TiposRespuestaPreguntaSeeder"/> las sincroniza a la BD.</summary>
public static class CatalogoTiposRespuestaPregunta
{
    public static readonly (
        string Codigo,
        string Nombre,
        string InstruccionUi,
        bool RequiereOpciones,
        short? MaxCaracteresRespuesta)[] Filas =
    [
        (TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve,
            "Respuesta breve",
            TipoRespuestaPreguntaCatalogo.InstruccionUiPorDefecto(TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve),
            false,
            500),
        (TipoRespuestaPreguntaCatalogo.CodigoSeleccionUnica,
            "Selección única",
            TipoRespuestaPreguntaCatalogo.InstruccionUiPorDefecto(TipoRespuestaPreguntaCatalogo.CodigoSeleccionUnica),
            true,
            null),
        (TipoRespuestaPreguntaCatalogo.CodigoSeleccionMultiple,
            "Selección múltiple",
            TipoRespuestaPreguntaCatalogo.InstruccionUiPorDefecto(TipoRespuestaPreguntaCatalogo.CodigoSeleccionMultiple),
            true,
            null),
    ];
}
