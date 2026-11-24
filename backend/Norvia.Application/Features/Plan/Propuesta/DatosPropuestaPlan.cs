using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Plan.Propuesta;

/// <summary>
/// Lo que el plan todavía no tiene: por cada riesgo confirmado y evaluado, sus controles sin tarea y las plantillas
/// de su peligro sin tarea. <paramref name="RiesgosSinEvaluar"/> cuenta los confirmados que quedan fuera por no tener nivel.
/// </summary>
public sealed record DatosPropuestaPlan(IReadOnlyList<RiesgoParaPlan> Riesgos, int RiesgosSinEvaluar);

/// <summary>Riesgo de la matriz con su última evaluación inicial (la prioridad sale de su nivel).</summary>
public sealed record RiesgoParaPlan(
    Guid Id,
    string Nombre,
    short Probabilidad,
    short Severidad,
    DateTime CreadoEn,
    IReadOnlyList<ControlParaPlan> Controles,
    IReadOnlyList<PlantillaParaPlan> Plantillas);

/// <summary>Control sin tarea. Si salió de una sugerencia, trae el título y la evidencia que pide su plantilla.</summary>
public sealed record ControlParaPlan(
    Guid Id, TipoControl Tipo, string Descripcion, DateTime CreadoEn, Guid? PlantillaId, string? PlantillaTitulo, bool RequiereEvidencia);

/// <summary>Tarea modelo del peligro (plantilla activa sin tipo de control) que el riesgo todavía no tiene.</summary>
public sealed record PlantillaParaPlan(Guid Id, string Titulo, string? Descripcion, short PrioridadSugerida, bool RequiereEvidencia);
