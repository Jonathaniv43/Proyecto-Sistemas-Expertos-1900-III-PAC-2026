using FluentValidation;

namespace Norvia.Application.Common.Validacion;

/// <summary>
/// Política de contraseñas de Norvia (HU-114). Se aplica en todo lugar donde se fija una contraseña
/// (registro de empresa, alta de vínculos y encuestados, cambio de contraseña); no en el login,
/// que solo compara con lo guardado. El front repite las mismas reglas en su esquema Zod.
/// </summary>
public static class PoliticaContrasena
{
    public const int Minimo = 8;
    public const int Maximo = 64;

    /// <summary>Cada requisito falla con su propio mensaje, para indicar qué hay que corregir.</summary>
    public static IRuleBuilderOptions<T, string> CumplePoliticaContrasena<T>(this IRuleBuilder<T, string> regla) =>
        regla
            .NotEmpty().WithMessage("Ingrese una contraseña.")
            .Length(Minimo, Maximo).WithMessage($"La contraseña debe tener entre {Minimo} y {Maximo} caracteres.")
            .Must(TieneMayuscula).WithMessage("La contraseña debe incluir al menos una letra mayúscula.")
            .Must(TieneNumero).WithMessage("La contraseña debe incluir al menos un número.")
            .Must(TieneEspecial).WithMessage("La contraseña debe incluir al menos un carácter especial (por ejemplo ! @ # $ %).")
            .Must(NoEsComun).WithMessage("Esa contraseña es muy común o conocida; elija otra.");

    private static bool TieneMayuscula(string? p) => p is not null && p.Any(char.IsUpper);
    private static bool TieneNumero(string? p) => p is not null && p.Any(c => c is >= '0' and <= '9');
    private static bool TieneEspecial(string? p) => p is not null && p.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
    private static bool NoEsComun(string? p) => p is null || !Comunes.Contains(p.Trim());

    /// <summary>
    /// Contraseñas comunes que sí cumplen los requisitos de composición, así que hay que rechazarlas aparte.
    /// La comparación ignora mayúsculas y minúsculas.
    /// </summary>
    private static readonly HashSet<string> Comunes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password1!", "Password1@", "Password123!", "Password@123", "P@ssw0rd", "P@ssw0rd1", "P@ssword1", "P@ssword123",
        "Passw0rd!", "Contraseña1!", "Contrasena1!", "Contraseña123!", "Contrasena123!", "Clave123!", "Clave@123",
        "Admin123!", "Admin@123", "Admin1234!", "Administrador1!", "Qwerty123!", "Qwerty1!", "Qwerty@123",
        "Welcome1!", "Welcome123!", "Bienvenido1!", "Bienvenido123!", "Abc123!", "Abc12345!", "Abcd1234!", "Abc@1234",
        "Aa123456!", "Aa12345678!", "Test123!", "Prueba123!", "Usuario123!", "User123!", "Root123!", "Hola1234!",
        "Honduras1!", "Honduras123!", "Tegucigalpa1!", "Norvia123!", "Norvia2026!", "Empresa123!", "Seguridad1!",
        "Changeme1!", "Letmein1!", "Iloveyou1!", "Teamo123!", "Summer2026!", "Verano2026!", "Enero2026!",
        "Football1!", "Futbol123!", "Monkey123!", "Dragon123!", "Master123!", "Sunshine1!", "Princess1!",
        "Zaq12wsx!", "1Qaz2wsx!", "1q2w3e4r!", "1Q2w3e4r!", "Q1w2e3r4!", "123456Aa!", "12345678Aa!", "Temporal123!",
    };
}
