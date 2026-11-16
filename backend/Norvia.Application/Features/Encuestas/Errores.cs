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
}
