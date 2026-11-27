namespace Norvia.Application.Common.Correo;

/// <summary>Sección <c>Correo</c> (variables <c>Correo__*</c> del .env).</summary>
public sealed class CorreoOpciones
{
    public const string Seccion = "Correo";

    public string ApiKeyResend { get; set; } = "";
    public string Remitente { get; set; } = "noreply@norvia.local";
    public string RemitenteNombre { get; set; } = "Norvia";
    public string UrlBaseFrontend { get; set; } = "http://localhost:5173";
}
