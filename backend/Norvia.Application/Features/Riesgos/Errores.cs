using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Riesgos;

/// <summary>Errores de negocio del módulo Riesgos, compartidos solo por sus slices.</summary>
public static class Errores
{
    public static readonly Error RiesgoNoEncontrado = new("riesgos.no_encontrado",
        "El riesgo no existe en esta empresa.", TipoError.NoEncontrado);

    public static readonly Error RiesgoFueraDeMatriz = new("riesgos.no_en_matriz",
        "El riesgo no está en la matriz: primero hay que confirmarlo.", TipoError.Conflicto);

    public static readonly Error PeligroInvalido = new("riesgos.peligro_invalido",
        "El peligro indicado no existe o ya no está disponible en el catálogo.", TipoError.Validacion);

    public static readonly Error RiesgoDuplicado = new("riesgos.duplicado",
        "Ese riesgo ya está en la matriz con la misma actividad.", TipoError.Conflicto);

    public static readonly Error SugerenciaInvalida = new("riesgos.sugerencia_invalida",
        "La sugerencia no corresponde al peligro de este riesgo o ya no está disponible.", TipoError.Validacion);

    public static readonly Error SugerenciaYaAceptada = new("riesgos.sugerencia_aceptada",
        "Esa sugerencia ya es un control de este riesgo.", TipoError.Conflicto);

    public static readonly Error SinEvaluacionInicial = new("riesgos.sin_evaluacion_inicial",
        "Primero hay que hacer la evaluación inicial del riesgo.", TipoError.Conflicto);

    public static readonly Error SinControles = new("riesgos.sin_controles",
        "El riesgo todavía no tiene controles: la evaluación residual mide el riesgo con los controles aplicados.", TipoError.Conflicto);
}
