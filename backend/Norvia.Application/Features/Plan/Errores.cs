using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Plan;

/// <summary>Errores de negocio del módulo Plan de trabajo, compartidos solo por sus slices.</summary>
public static class Errores
{
    public static readonly Error SinVinculo = new("plan.sin_vinculo",
        "El plan de trabajo es de una empresa: inicie sesión con su vínculo.", TipoError.Prohibido);

    public static readonly Error TareaNoEncontrada = new("plan.tarea_no_encontrada",
        "La tarea no existe en esta empresa.", TipoError.NoEncontrado);

    public static readonly Error TareaCerrada = new("plan.tarea_cerrada",
        "La tarea ya está completada o cancelada: no se reasigna.", TipoError.Conflicto);

    public static readonly Error ResponsableInvalido = new("plan.responsable_invalido",
        "El responsable tiene que ser una persona activa de la empresa (no un encuestado).", TipoError.Validacion);

    public static readonly Error FechaPasada = new("plan.fecha_pasada",
        "La fecha límite no puede ser anterior a hoy.", TipoError.Validacion);

    public static readonly Error TareaAjena = new("plan.tarea_ajena",
        "Solo la persona responsable de la tarea o el Responsable SST pueden cambiar su estado.", TipoError.Prohibido);

    public static readonly Error TareaSinAsignar = new("plan.tarea_sin_asignar",
        "La tarea necesita responsable y fecha límite antes de iniciarse.", TipoError.Conflicto);

    public static readonly Error FaltaEvidencia = new("plan.falta_evidencia",
        "La tarea pide evidencia: adjúntela antes de completarla.", TipoError.Conflicto);

    public static readonly Error TareaCancelada = new("plan.tarea_cancelada",
        "La tarea está cancelada: su estado ya no cambia.", TipoError.Conflicto);
}
