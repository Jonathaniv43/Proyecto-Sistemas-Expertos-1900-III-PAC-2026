using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Documentos;

/// <summary>Errores de negocio del módulo Documentos (M9), compartidos solo por sus slices.</summary>
public static class Errores
{
    public static readonly Error SinVinculo = new("documentos.sin_vinculo",
        "Los documentos son de una empresa: inicie sesión con su vínculo.", TipoError.Prohibido);

    /// <summary>Las plantillas se filtran por rubro (HU-070): sin rubro no hay cuáles mostrar.</summary>
    public static readonly Error SinRubro = new("documentos.sin_rubro",
        "Para generar documentos, primero indique el rubro en el perfil de la empresa.", TipoError.Conflicto);

    /// <summary>No existe, está desactivada o es de otro rubro: se responde igual en los tres casos.</summary>
    public static readonly Error PlantillaNoDisponible = new("documentos.plantilla_no_disponible",
        "La plantilla no existe o no está disponible para el rubro de la empresa.", TipoError.NoEncontrado);

    public static readonly Error DocumentoNoEncontrado = new("documentos.no_encontrado",
        "El documento no existe.", TipoError.NoEncontrado);

    /// <summary>RN-50: solo se edita la versión en borrador; la vigente y las reemplazadas no cambian.</summary>
    public static readonly Error SinBorrador = new("documentos.sin_borrador",
        "El documento no tiene un borrador: una versión aprobada no se edita (RN-50).", TipoError.Conflicto);

    /// <summary>RN-49: una versión aprobada ya no cambia, así que no se aprueba con datos por completar.</summary>
    public static readonly Error PorCompletar = new("documentos.por_completar",
        "El documento tiene datos por completar: complételos en el borrador antes de aprobarlo.", TipoError.Conflicto);

    /// <summary>HU-073: solo se admite un borrador a la vez por documento.</summary>
    public static readonly Error YaTieneBorrador = new("documentos.ya_tiene_borrador",
        "El documento ya tiene un borrador abierto. Edítelo o apruébelo antes de crear otra versión.", TipoError.Conflicto);

    /// <summary>HU-073: la versión nueva parte de la versión vigente.</summary>
    public static readonly Error SinVigente = new("documentos.sin_vigente",
        "El documento todavía no tiene una versión vigente desde la cual crear una versión nueva.", TipoError.Conflicto);

    public static readonly Error VersionNoEncontrada = new("documentos.version_no_encontrada",
        "La versión del documento no existe.", TipoError.NoEncontrado);
}
