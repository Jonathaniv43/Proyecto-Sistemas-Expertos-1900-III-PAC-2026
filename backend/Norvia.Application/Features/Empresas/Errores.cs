using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Empresas;

/// <summary>Errores de negocio del módulo Empresas, compartidos solo por sus slices.</summary>
public static class Errores
{
    public static readonly Error RtnDuplicado = new("empresas.rtn_duplicado", "Ya existe una empresa registrada con ese RTN.", TipoError.Conflicto);

    public static readonly Error EmailDuplicado = new("empresas.email_duplicado", "Ya existe una cuenta registrada con ese correo electrónico.", TipoError.Conflicto);

    public static readonly Error DocumentoDuplicado = new("empresas.documento_duplicado", "Ya existe una persona registrada con ese documento de identidad.", TipoError.Conflicto);

    public static readonly Error RubroInvalido = new("empresas.rubro_invalido", "El rubro indicado no existe o no está activo.", TipoError.Validacion);

    public static readonly Error TipoDocumentoInvalido = new("empresas.tipo_documento_invalido", "El tipo de documento no es válido.", TipoError.Validacion);

    public static readonly Error PlantillaCorreoNoEncontrada = new(
        "empresas.plantilla_correo_no_encontrada",
        "No está configurada la plantilla de correo de bienvenida.",
        TipoError.Servidor);
}