namespace Norvia.Infrastructure.Opciones;

/// <summary>Sección <c>StaffSeed</c> (variables <c>StaffSeed__*</c> del .env).</summary>
public sealed class StaffSeedOpciones
{
    public const string Seccion = "StaffSeed";

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
