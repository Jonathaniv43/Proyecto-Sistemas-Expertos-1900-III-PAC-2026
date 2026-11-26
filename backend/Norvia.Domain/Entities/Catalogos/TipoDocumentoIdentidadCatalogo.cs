namespace Norvia.Domain.Entities.Catalogos;

/// <summary>
/// Catálogo cerrado de tipos de documento de identidad (tabla <c>TipoDocumentoIdentidad</c>).
/// Los códigos coinciden con <see cref="Enums.TipoDocumentoIdentidad"/> en Persona (snake_case).
/// </summary>
public class TipoDocumentoIdentidadCatalogo
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}