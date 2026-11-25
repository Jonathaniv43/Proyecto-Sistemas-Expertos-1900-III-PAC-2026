using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Documentos.ListarPlantillasDisponibles;

/// <summary>Una plantilla que la empresa puede usar para generar un documento (HU-070).</summary>
public sealed record PlantillaDisponibleResponseDto(Guid Id, string Codigo, string Titulo, TipoDocumento TipoDocumento);
