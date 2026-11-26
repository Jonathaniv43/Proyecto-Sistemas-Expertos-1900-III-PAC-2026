namespace Norvia.Application.Features.Empresas.ListarEmpresasPlataforma.DTOs;

/// <summary>Filtro de la consulta de empresas en plataforma (HU-024).</summary>
public sealed record FiltrosEmpresasPlataformaDto
{
    /// <summary>Texto en la razón social o nombre del rubro (sin distinguir mayúsculas). Opcional.</summary>
    public string? Buscar { get; init; }
}
