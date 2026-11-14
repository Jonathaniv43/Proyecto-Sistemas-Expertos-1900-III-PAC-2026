using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Auth;

/// <summary>Errores de negocio del módulo Auth, compartidos solo por sus slices.</summary>
public static class Errores
{
    /// <summary>Mismo mensaje para cualquier dato incorrecto: no revela qué falló (HU-018).</summary>
    public static readonly Error CredencialesInvalidas =
        new("auth.credenciales_invalidas", "Las credenciales no son válidas.", TipoError.NoAutorizado);

    public static readonly Error SesionInvalida =
        new("auth.sesion_invalida", "La sesión no es válida o expiró.", TipoError.NoAutorizado);
}
