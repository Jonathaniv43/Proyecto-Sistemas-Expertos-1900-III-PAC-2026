namespace Norvia.Application.Features.Encuestas.ListarEncuestasPendientes.DTOs;

public sealed record EncuestaPendienteOpcionResponseDto(Guid Id, string Texto);

public sealed record EncuestaPendientePreguntaResponseDto(
    Guid Id,
    string Texto,
    string TipoRespuesta,
    string InstruccionUi,
    bool Obligatoria,
    short? MaxCaracteres,
    IReadOnlyList<EncuestaPendienteOpcionResponseDto> Opciones);

/// <summary>Encuesta publicada pendiente de responder por el vínculo de la sesión (HU-031).</summary>
public sealed record EncuestaPendienteResponseDto(
    Guid EncuestaId,
    string Titulo,
    Guid VersionId,
    string? Instrucciones,
    IReadOnlyList<EncuestaPendientePreguntaResponseDto> Preguntas);
