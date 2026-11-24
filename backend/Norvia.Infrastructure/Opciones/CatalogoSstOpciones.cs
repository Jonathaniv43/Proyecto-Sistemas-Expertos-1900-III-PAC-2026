namespace Norvia.Infrastructure.Opciones;

/// <summary>Sección <c>CatalogoSst</c>: cargar o no el contenido SST inicial al arrancar (HU-097).</summary>
public sealed class CatalogoSstOpciones
{
    public const string Seccion = "CatalogoSst";

    /// <summary>Por defecto, sí. Las pruebas de integración lo apagan para trabajar con catálogos vacíos.</summary>
    public bool Cargar { get; set; } = true;
}
