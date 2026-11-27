using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Documentos.DTOs;

/// <summary>Un documento vigente en la lista para todas las personas de la empresa (HU-074).</summary>
public sealed record DocumentoVigenteResumenResponseDto(
    Guid Id,
    string Titulo,
    TipoDocumento TipoDocumento,
    int NumeroVersion,
    DateTime VigenteDesde);

/// <summary>
/// La versión vigente de un documento (HU-074): solo lo que se publica. No incluye borradores, plantilla ni quién lo
/// generó.
/// </summary>
public sealed record DocumentoVigenteResponseDto(
    Guid Id,
    string Titulo,
    TipoDocumento TipoDocumento,
    int NumeroVersion,
    string Contenido,
    DateTime VigenteDesde,
    string AprobadaPorNombre);
