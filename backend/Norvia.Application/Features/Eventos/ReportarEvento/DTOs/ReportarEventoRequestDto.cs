using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Eventos.ReportarEvento.DTOs;

/// <summary>HU-047: datos para reportar de forma anónima un evento SST (RN-26, D11).</summary>
public sealed record ReportarEventoRequestDto
{
    public TipoAccidente? Tipo { get; init; }
    public EstadoEnfermedad? EstadoEnfermedad { get; init; }
    public bool EsGrave { get; init; }
    public DateTime? FechaHoraEvento { get; init; }
    public string? Lugar { get; init; }
    public string Descripcion { get; init; } = string.Empty;
}
