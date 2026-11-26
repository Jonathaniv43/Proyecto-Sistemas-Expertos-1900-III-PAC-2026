using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Encuestas;

/// <summary>Errores de negocio del módulo Encuestas, compartidos solo por sus slices.</summary>
public static class Errores
{
    /// <summary>RN-10: sin rubro no hay categorías que elegir ni encuesta que generar.</summary>
    public static readonly Error EmpresaSinRubro = new("encuestas.empresa_sin_rubro",
        "La empresa todavía no tiene rubro. Complete el perfil de la empresa para elegir categorías de riesgo.",
        TipoError.Conflicto);

    public static readonly Error CategoriaInvalida = new("encuestas.categoria_invalida",
        "Una o más categorías seleccionadas no existen, están desactivadas o no corresponden al rubro de la empresa.",
        TipoError.Validacion);

    public static readonly Error EncuestaNoEncontrada = new("encuestas.no_encontrada",
        "La encuesta no existe.",
        TipoError.NoEncontrado);

    public static readonly Error VersionNoEncontrada = new("encuestas.version_no_encontrada",
        "La versión de la encuesta no existe.",
        TipoError.NoEncontrado);

    public static readonly Error SoloCreadorPuedeEditar = new("encuestas.solo_creador_puede_editar",
        "No puedes modificar esta encuesta porque fue creada por otro responsable.",
        TipoError.Prohibido);

    public static readonly Error EncuestaCerrada = new("encuestas.cerrada",
        "La encuesta está cerrada y no permite modificaciones ni publicaciones.",
        TipoError.Conflicto);

    public static readonly Error VersionNoPublicable = new("encuestas.version_no_publicable",
        "Solo se puede publicar una versión que se encuentre en estado borrador.",
        TipoError.Conflicto);

    public static readonly Error EncuestaSinPreguntas = new("encuestas.sin_preguntas",
        "La versión debe tener al menos una pregunta antes de publicarse.",
        TipoError.Conflicto);

    public static readonly Error PeligroInvalido = new("encuestas.peligro_invalido",
        "Uno o más peligros indicados no existen o están desactivados.",
        TipoError.Validacion);

    public static readonly Error TipoRespuestaInvalido = new("encuestas.tipo_respuesta_invalido",
        "El tipo de respuesta indicado no existe.",
        TipoError.Validacion);

    public static readonly Error YaParticipo = new("encuestas.ya_participo",
        "Ya respondiste esta encuesta. Solo se permite una participación por persona.",
        TipoError.Conflicto);

    public static readonly Error VersionObsoleta = new("encuestas.version_obsoleta",
        "La versión indicada no es la versión publicada vigente de esta encuesta.",
        TipoError.Conflicto);

    public static readonly Error EncuestaSinVersionPublicada = new("encuestas.sin_version_publicada",
        "La encuesta no tiene una versión publicada para consultar o responder.",
        TipoError.Conflicto);

    public static readonly Error RespuestaInvalida = new("encuestas.respuesta_invalida",
        "Las respuestas no cumplen las reglas de las preguntas (obligatorias, tipo u opciones).",
        TipoError.Validacion);

    public static readonly Error SugerenciaNoEncontrada = new("encuestas.sugerencia_no_encontrada",
        "El peligro sugerido no existe en esta encuesta.",
        TipoError.NoEncontrado);

    public static readonly Error SugerenciaYaRevisada = new("encuestas.sugerencia_ya_revisada",
        "Este peligro sugerido ya fue confirmado o descartado.",
        TipoError.Conflicto);

    public static readonly Error VersionNoComparable = new("encuestas.version_no_comparable",
        "Debes elegir dos versiones distintas que ya hayan sido publicadas para compararlas.",
        TipoError.Validacion);

    public static readonly Error VersionSinPublicar = new("encuestas.version_sin_publicar",
        "No se pueden consultar resultados de una versión que sigue en borrador.",
        TipoError.Conflicto);

    public static Error RespuestaInvalidaDetalle(string detalle) =>
        new("encuestas.respuesta_invalida", detalle, TipoError.Validacion);
}
