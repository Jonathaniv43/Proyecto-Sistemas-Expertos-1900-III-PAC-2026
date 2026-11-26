namespace Norvia.Application.Features.Empresas.ActualizarPerfilEmpresa.DTOs;

/// <summary>Edición parcial del perfil (HU-026). Se envía solo lo que cambia. RTN y estado no se editan aquí.</summary>
public sealed record ActualizarPerfilEmpresaRequestDto
{
    public string? RazonSocial { get; init; }
    public string? Direccion { get; init; }
    public string? ActividadEconomica { get; init; }
    /// <summary>Código del rubro en catálogo (tabla Rubro), p. ej. <c>comercio</c>.</summary>
    public string? RubroCodigo { get; init; }
    public int? TrabajadoresPermanentes { get; init; }
    public DateOnly? FechaInicioOperaciones { get; init; }
}
