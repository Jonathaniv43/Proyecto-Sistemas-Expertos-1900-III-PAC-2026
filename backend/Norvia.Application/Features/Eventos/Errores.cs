using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Eventos;

/// <summary>Errores de negocio del módulo Eventos / M6, compartidos solo por sus slices.</summary>
public static class Errores
{
    public static readonly Error EventoNoEncontrado = new("eventos.no_encontrado",
        "El evento no existe.",
        TipoError.NoEncontrado);

    public static readonly Error FechaEventoFutura = new("eventos.fecha_futura",
        "La fecha y hora del evento no puede ser futura.",
        TipoError.Validacion);

    public static Error FiltroInvalido(string campo, string valor) => new("eventos.filtro_invalido",
        $"El filtro '{campo}' no acepta el valor '{valor}'.",
        TipoError.Validacion);

    public static readonly Error EventoTerminado = new("eventos.terminado",
        "El evento ya está cerrado o descartado.", TipoError.Conflicto);

    public static readonly Error InvestigadorInvalido = new("eventos.investigador_invalido",
        "Quien investiga tiene que ser una persona activa de la empresa (no un encuestado).", TipoError.Validacion);

    public static readonly Error FechaInvestigacionInvalida = new("eventos.fecha_investigacion_invalida",
        "La fecha de la investigación no puede ser futura ni anterior al evento.", TipoError.Validacion);

    public static readonly Error SinInvestigacion = new("eventos.sin_investigacion",
        "Para cerrar el evento hay que registrar su investigación (RN-28).", TipoError.Conflicto);

    public static readonly Error SinMedidas = new("eventos.sin_medidas",
        "Para cerrar el evento hace falta al menos una medida correctiva (RN-28).", TipoError.Conflicto);
}
