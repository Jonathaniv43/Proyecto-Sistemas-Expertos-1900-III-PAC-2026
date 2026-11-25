namespace Norvia.Application.Features.Documentos.GenerarDocumento.DTOs;

/// <summary>Generar un documento desde una plantilla (HU-070).</summary>
public sealed record GenerarDocumentoRequestDto
{
    public Guid? PlantillaDocumentoId { get; init; }

    /// <summary>Opcional: si no se envía, el documento toma el título de la plantilla.</summary>
    public string? Titulo { get; init; }
}
