using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Plan.Propuesta;

/// <summary>De dónde sale una tarea propuesta.</summary>
public enum OrigenPropuesta
{
    /// <summary>Implementa un control del riesgo (HU-039).</summary>
    Control,

    /// <summary>Tarea modelo de una plantilla del peligro.</summary>
    Plantilla
}

/// <summary>Tarea que el plan propone y todavía no existe. <paramref name="Clave"/> la identifica para confirmarla.</summary>
public sealed record TareaPropuesta(
    string Clave,
    Guid RiesgoId,
    string RiesgoNombre,
    int RiesgoNivel,
    ClasificacionRiesgo RiesgoClasificacion,
    OrigenPropuesta Origen,
    Guid? ControlId,
    TipoControl? TipoControl,
    Guid? PlantillaId,
    string Titulo,
    string? Descripcion,
    short Prioridad,
    bool RequiereEvidencia);

/// <summary>
/// HU-042: cómo los riesgos y sus controles se vuelven tareas (art. 46.1.e). Es determinista: los mismos datos
/// dan la misma propuesta, y la misma clave para cada tarea.
/// <list type="bullet">
/// <item>Cada control sin tarea produce una tarea que lo implementa. Si el control salió de una sugerencia, la tarea
/// usa el título de su plantilla y la evidencia que pide.</item>
/// <item>Cada plantilla activa del peligro <b>sin</b> tipo de control (tarea modelo, no un control) produce una tarea.
/// Las que sugieren un control solo entran si el control se aceptó (RN-76).</item>
/// <item>Prioridad por el nivel inicial del riesgo: alto o crítico 1, medio 2, bajo 3.</item>
/// <item>Orden: los riesgos de nivel más alto primero; dentro de cada uno, los controles por efectividad (RN-21) y
/// luego las tareas modelo por su prioridad sugerida.</item>
/// </list>
/// </summary>
public static class PropuestaPlan
{
    public static string ClaveControl(Guid controlId) => $"control:{controlId}";
    public static string ClavePlantilla(Guid riesgoId, Guid plantillaId) => $"plantilla:{riesgoId}:{plantillaId}";

    public static short PrioridadDe(ClasificacionRiesgo clasificacion) => clasificacion switch
    {
        ClasificacionRiesgo.Critico or ClasificacionRiesgo.Alto => 1,
        ClasificacionRiesgo.Medio => 2,
        _ => 3
    };

    public static IReadOnlyList<TareaPropuesta> Calcular(IEnumerable<RiesgoParaPlan> riesgos) =>
        riesgos
            .OrderByDescending(r => r.Probabilidad * r.Severidad).ThenByDescending(r => r.CreadoEn).ThenBy(r => r.Id)
            .SelectMany(TareasDe)
            .ToList();

    private static IEnumerable<TareaPropuesta> TareasDe(RiesgoParaPlan r)
    {
        var nivel = r.Probabilidad * r.Severidad;
        var clasificacion = EvaluacionRiesgo.Clasificar(nivel);
        var prioridad = PrioridadDe(clasificacion);

        foreach (var c in r.Controles.OrderBy(c => c.Tipo).ThenBy(c => c.CreadoEn).ThenBy(c => c.Id))
            yield return new TareaPropuesta(ClaveControl(c.Id), r.Id, r.Nombre, nivel, clasificacion, OrigenPropuesta.Control,
                c.Id, c.Tipo, c.PlantillaId, c.PlantillaTitulo ?? Titulo(c.Descripcion), c.Descripcion, prioridad, c.RequiereEvidencia);

        foreach (var p in r.Plantillas.OrderBy(p => p.PrioridadSugerida).ThenBy(p => p.Titulo, StringComparer.CurrentCulture).ThenBy(p => p.Id))
            yield return new TareaPropuesta(ClavePlantilla(r.Id, p.Id), r.Id, r.Nombre, nivel, clasificacion, OrigenPropuesta.Plantilla,
                null, null, p.Id, p.Titulo, p.Descripcion, prioridad, p.RequiereEvidencia);
    }

    /// <summary>Título de una tarea a partir de la descripción de un control propio: su primera línea, hasta 200 caracteres.</summary>
    internal static string Titulo(string descripcion)
    {
        var linea = descripcion.Split('\n', 2)[0].Trim();
        if (linea.Length <= 200) return linea;
        var corte = linea.LastIndexOf(' ', 198);
        return (corte > 100 ? linea[..corte] : linea[..199]).TrimEnd() + "…";
    }
}
