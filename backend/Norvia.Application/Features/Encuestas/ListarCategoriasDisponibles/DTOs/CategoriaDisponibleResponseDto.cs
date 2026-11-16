using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.ListarCategoriasDisponibles.DTOs;

public sealed record OpcionPreviewDto(string Texto, short Orden);

/// <summary>Pregunta tal como se incorporará a la encuesta (HU-029: vista previa antes de crearla).</summary>
public sealed record PreguntaPreviewDto(
    Guid Id, string Texto, TipoRespuestaPregunta TipoRespuesta, bool Obligatoria, short Orden,
    IReadOnlyList<OpcionPreviewDto> Opciones);

/// <summary>Categoría de riesgo del rubro de la empresa, con sus preguntas activas.</summary>
public sealed record CategoriaDisponibleResponseDto(
    Guid Id, string Nombre, string? Descripcion, IReadOnlyList<PreguntaPreviewDto> PreguntasPreview);
