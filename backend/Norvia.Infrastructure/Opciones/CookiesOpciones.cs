namespace Norvia.Infrastructure.Opciones;

/// <summary>Sección <c>Cookies</c> (variables <c>Cookies__*</c> del .env de Norvia.API).</summary>
public sealed class CookiesOpciones
{
    public const string Seccion = "Cookies";

    public string RefreshName { get; set; } = "norvia_refresh";
    public string AccessName { get; set; } = "norvia_access";
    public string Domain { get; set; } = "localhost";
    public string Path { get; set; } = "/";
    /// <summary>Lax | Strict | None (texto; la API lo convierte a <c>SameSiteMode</c>).</summary>
    public string SameSite { get; set; } = "Lax";
    public bool Secure { get; set; }
    public bool HttpOnly { get; set; } = true;
}
