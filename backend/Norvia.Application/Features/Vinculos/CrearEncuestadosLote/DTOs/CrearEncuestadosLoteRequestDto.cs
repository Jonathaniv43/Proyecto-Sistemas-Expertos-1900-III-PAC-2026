namespace Norvia.Application.Features.Vinculos.CrearEncuestadosLote.DTOs;

public sealed record FilaEncuestadoDto
{
    public string Email { get; init; } = string.Empty;
    public string PrimerNombre { get; init; } = string.Empty;
    public string? SegundoNombre { get; init; }
    public string PrimerApellido { get; init; } = string.Empty;
    public string? SegundoApellido { get; init; }
}

/// <summary>Lista de trabajadores a dar de alta como encuestados (HU-115). Máximo 500 filas por envío.</summary>
public sealed record CrearEncuestadosLoteRequestDto
{
    public IReadOnlyList<FilaEncuestadoDto> Filas { get; init; } = [];
}
