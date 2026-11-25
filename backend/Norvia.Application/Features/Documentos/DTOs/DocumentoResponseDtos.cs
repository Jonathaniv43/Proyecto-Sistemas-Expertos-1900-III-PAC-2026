using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Documentos.DTOs;

/// <summary>
/// Un documento en la lista (HU-070). <see cref="NumeroVersion"/> y <see cref="Estado"/> son los de su versión de
/// trabajo: el borrador si hay uno; si no, la vigente.
/// </summary>
public sealed record DocumentoResumenResponseDto(
    Guid Id,
    string Titulo,
    TipoDocumento TipoDocumento,
    int NumeroVersion,
    EstadoVersionDocumento Estado,
    DateTime ActualizadoEn);

/// <summary>Una versión de un documento, con su contenido en Markdown.</summary>
public sealed record VersionDocumentoResponseDto(
    Guid Id,
    int NumeroVersion,
    EstadoVersionDocumento Estado,
    string Contenido,
    DateTime CreadaEn,
    string CreadaPorNombre,
    DateTime? AprobadaEn,
    string? AprobadaPorNombre);

/// <summary>
/// Un documento con su versión de trabajo (el borrador si hay uno; si no, la vigente). <see cref="PorCompletar"/> lista
/// los <c>[Completar: …]</c> que quedan en esa versión, para avisar qué datos faltan (RN-49).
/// </summary>
public sealed record DocumentoDetalleResponseDto(
    Guid Id,
    string Titulo,
    TipoDocumento TipoDocumento,
    string? PlantillaTitulo,
    DateTime CreadoEn,
    VersionDocumentoResponseDto Version,
    IReadOnlyList<string> PorCompletar);
