namespace Norvia.Domain.Entities.Catalogos;

/// <summary>
/// Catálogo cerrado de tipos de respuesta (tabla <c>TipoRespuestaPregunta</c>).
/// Fuente de verdad en BD; en código solo se declaran códigos para sembrar y comparar.
/// </summary>
public class TipoRespuestaPreguntaCatalogo
{
    public const string CodigoRespuestaBreve = "respuesta_breve";
    public const string CodigoSeleccionUnica = "seleccion_unica";
    public const string CodigoSeleccionMultiple = "seleccion_multiple";

    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string InstruccionUi { get; set; } = string.Empty;
    public bool RequiereOpciones { get; set; }
    public short? MaxCaracteresRespuesta { get; set; }
    public bool Activo { get; set; } = true;

    public static string NormalizarCodigo(string? codigo) =>
        codigo?.Trim().ToLowerInvariant() ?? string.Empty;

    public static bool EsCodigoConocido(string? codigo) =>
        NormalizarCodigo(codigo) is CodigoRespuestaBreve or CodigoSeleccionUnica or CodigoSeleccionMultiple;

    public static bool EsCodigoRespuestaBreve(string? codigo) =>
        NormalizarCodigo(codigo) == CodigoRespuestaBreve;

    public static bool CodigoRequiereOpciones(string? codigo) =>
        NormalizarCodigo(codigo) is CodigoSeleccionUnica or CodigoSeleccionMultiple;

    public static string InstruccionUiPorDefecto(string? codigo) => NormalizarCodigo(codigo) switch
    {
        CodigoRespuestaBreve => "Responda brevemente lo que se le pide (máximo 500 caracteres).",
        CodigoSeleccionUnica => "Seleccione una opción.",
        CodigoSeleccionMultiple => "Seleccione una o más opciones.",
        _ => string.Empty
    };
}
