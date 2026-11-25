namespace Norvia.Infrastructure.Opciones;

/// <summary>Sección <c>Microsoft</c> (variables <c>Microsoft__*</c> del .env, D29).</summary>
public sealed class MicrosoftOpciones
{
    public const string Seccion = "Microsoft";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>"common" acepta cuentas de trabajo y personales; un GUID restringe a un solo directorio.</summary>
    public string Tenant { get; set; } = "common";

    /// <summary>URIs de redirección permitidas, separadas por coma (las registradas en Azure).</summary>
    public string RedirectUris { get; set; } = string.Empty;

    public bool EstaConfigurado => !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);

    public bool RedirectPermitido(string uri) =>
        RedirectUris.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains(uri, StringComparer.Ordinal);
}
