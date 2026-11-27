namespace Norvia.Application.Features.Documentos.ListarVigentes.DTOs;

/// <summary>Filtro de la lista de documentos vigentes (HU-074), como parámetro de la consulta.</summary>
public sealed record FiltrosVigentesDto
{
    /// <summary>Texto en el título (sin distinguir mayúsculas). Opcional.</summary>
    public string? Buscar { get; init; }
}
