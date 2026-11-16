namespace Norvia.Application.Common.Correo;

/// <summary>Sección <c>Correo</c> (variables <c>Correo__*</c> del .env).</summary>
public sealed class CorreoOpciones
{
    public const string Seccion = "Correo";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public bool UseSsl { get; set; }
    public string? Usuario { get; set; }
    public string? Password { get; set; }
    public string Remitente { get; set; } = "noreply@norvia.local";
    public string RemitenteNombre { get; set; } = "Norvia";
    public string UrlBaseFrontend { get; set; } = "http://localhost:5173";
}