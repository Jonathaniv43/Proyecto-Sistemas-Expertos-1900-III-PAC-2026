using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Vinculos;

/// <summary>Errores de negocio del módulo Vinculos, compartidos solo por sus slices.</summary>
public static class Errores
{
    public static readonly Error VinculoDuplicado = new("vinculos.duplicado",
        "Esa persona ya tiene acceso a esta empresa. Para cambiar su rol o reactivarla, edite su acceso.", TipoError.Conflicto);

    public static readonly Error RolInvalido = new("vinculos.rol_invalido", "El rol indicado no existe.", TipoError.Validacion);

    public static readonly Error PasswordTemporalRequerida = new("vinculos.password_temporal_requerida",
        "El correo no tiene una cuenta en Norvia: indique una contraseña temporal para crearla.", TipoError.Validacion);

    public static readonly Error VinculoNoEncontrado = new("vinculos.no_encontrado",
        "La persona no tiene acceso a esta empresa.", TipoError.NoEncontrado);

    public static readonly Error UltimoAdministrador = new("vinculos.ultimo_administrador",
        "La empresa debe conservar al menos un administrador con acceso activo.", TipoError.Conflicto);
}
