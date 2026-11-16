namespace Norvia.Domain.Enums;

/// <summary>
/// Legacy: no usar en entidades nuevas. Códigos de negocio en
/// <see cref="Entities.Catalogos.TipoRespuestaPreguntaCatalogo"/>.
/// </summary>
[Obsolete("Usar TipoRespuestaPreguntaCatalogo y códigos string; se elimina cuando no queden referencias.")]
public enum TipoRespuestaPregunta
{
    RespuestaBreve,
    SeleccionUnica,
    SeleccionMultiple
}
