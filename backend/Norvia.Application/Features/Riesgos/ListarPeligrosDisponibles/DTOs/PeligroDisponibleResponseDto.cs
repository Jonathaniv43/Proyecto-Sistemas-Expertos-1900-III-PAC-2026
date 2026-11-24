using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.ListarPeligrosDisponibles.DTOs;

/// <summary>Peligro activo del catálogo, para elegirlo al agregar un riesgo (HU-036).</summary>
public sealed record PeligroDisponibleResponseDto(Guid Id, string Nombre, string? Descripcion, ClasificacionPeligro Clasificacion);
