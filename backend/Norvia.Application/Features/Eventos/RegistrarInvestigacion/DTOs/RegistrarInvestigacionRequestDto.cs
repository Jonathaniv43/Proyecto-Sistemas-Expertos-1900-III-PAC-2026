namespace Norvia.Application.Features.Eventos.RegistrarInvestigacion.DTOs;

/// <summary>Investigación de un evento (HU-049): causas inmediatas y de fondo, conclusiones, quién y cuándo.</summary>
public sealed record RegistrarInvestigacionRequestDto
{
    public string? CausasInmediatas { get; init; }

    /// <summary>Causas de fondo.</summary>
    public string? CausasBasicas { get; init; }
    public string? Conclusiones { get; init; }
    public Guid? InvestigadaPorId { get; init; }
    public DateOnly? FechaInvestigacion { get; init; }
}
