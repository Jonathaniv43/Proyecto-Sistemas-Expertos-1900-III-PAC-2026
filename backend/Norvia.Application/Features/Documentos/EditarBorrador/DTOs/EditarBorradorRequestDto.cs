namespace Norvia.Application.Features.Documentos.EditarBorrador.DTOs;

/// <summary>El contenido nuevo del borrador, en Markdown (HU-071). Reemplaza al anterior completo.</summary>
public sealed record EditarBorradorRequestDto
{
    public string? Contenido { get; init; }
}
