using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Catalogos.CrearPlantillaDocumento.DTOs;

public sealed record CrearPlantillaDocumentoRequestDto
{
    /// <summary>Identificador estable en minúsculas. No se puede cambiar después.</summary>
    public string Codigo { get; init; } = string.Empty;
    public string Titulo { get; init; } = string.Empty;
    public TipoDocumento TipoDocumento { get; init; }

    /// <summary>Nulo si aplica a todos los rubros.</summary>
    public short? RubroId { get; init; }

    /// <summary>Texto con marcadores como <c>{{RazonSocial}}</c>.</summary>
    public string Contenido { get; init; } = string.Empty;
}
