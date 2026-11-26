namespace Norvia.Application.Features.Eventos.DescartarEvento.DTOs;

/// <summary>Descartar un reporte falso, duplicado o inválido (HU-049, RN-28). El motivo es opcional.</summary>
public sealed record DescartarEventoRequestDto
{
    public string? Motivo { get; init; }
}
