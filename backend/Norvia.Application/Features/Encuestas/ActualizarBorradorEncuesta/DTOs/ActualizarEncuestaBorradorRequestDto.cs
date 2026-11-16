namespace Norvia.Application.Features.Encuestas.ActualizarBorradorEncuesta.DTOs;

public sealed record ActualizarEncuestaBorradorRequestDto
{
    public string? Titulo { get; init; }
    public string? Instrucciones { get; init; }
    public IReadOnlyList<SeccionBorradorRequestDto> Secciones { get; init; } = [];
    public IReadOnlyList<PreguntaBorradorRequestDto> Preguntas { get; init; } = [];
}

public sealed record SeccionBorradorRequestDto
{
    public Guid? Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public short Orden { get; init; }
}

public sealed record PreguntaBorradorRequestDto
{
    public Guid? Id { get; init; }
    public Guid? SeccionId { get; init; }
    public Guid? CategoriaRiesgoId { get; init; }
    public string Texto { get; init; } = string.Empty;
    public string? TipoRespuestaCodigo { get; init; }
    public bool Obligatoria { get; init; } = true;
    public Guid? PeligroId { get; init; }
    public short Orden { get; init; }
    public IReadOnlyList<OpcionBorradorRequestDto> Opciones { get; init; } = [];
}

public sealed record OpcionBorradorRequestDto
{
    public Guid? Id { get; init; }
    public string Texto { get; init; } = string.Empty;
    public short Orden { get; init; }
    public bool IndicaExposicion { get; init; }
}
