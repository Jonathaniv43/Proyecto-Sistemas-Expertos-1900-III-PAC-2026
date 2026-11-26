using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Domain.Entities.Documentos;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Documentos;

/// <summary>
/// Compartido por los repositorios de Documentos. Documento, VersionDocumento y VinculoEmpresa son tablas de empresa:
/// el filtro global limita a la sesión.
/// </summary>
internal static class ProyeccionDocumento
{
    /// <summary>
    /// Orden para elegir la versión de trabajo: el borrador si hay uno; si no, la vigente; si no, la más reciente.
    /// </summary>
    internal static readonly Expression<Func<VersionDocumento, int>> PrioridadDeTrabajo = v =>
        v.Estado == EstadoVersionDocumento.Borrador ? 0 : v.Estado == EstadoVersionDocumento.Vigente ? 1 : 2;

    /// <summary>
    /// El documento con su versión de trabajo; nulo si no existe en la empresa de la sesión.
    /// <see cref="DocumentoDetalleResponseDto.PorCompletar"/> va vacío: lo calcula el servicio desde el contenido.
    /// </summary>
    internal static async Task<DocumentoDetalleResponseDto?> DetalleAsync(NorviaDbContext db, Guid id, CancellationToken ct)
    {
        var fila = await db.Documento.AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new
            {
                d.Id, d.Titulo, d.TipoDocumento, d.CreadoEn,
                PlantillaTitulo = d.PlantillaDocumento != null ? d.PlantillaDocumento.Titulo : null,
                Version = d.Versiones.AsQueryable().OrderBy(PrioridadDeTrabajo).ThenByDescending(v => v.NumeroVersion)
                    .Select(v => new
                    {
                        v.Id, v.NumeroVersion, v.Estado, v.Contenido, v.CreadaEn, CreadaPor = v.CreadaPor.Persona,
                        v.AprobadaEn, AprobadaPor = v.AprobadaPor != null ? v.AprobadaPor.Persona : null
                    })
                    .First()
            })
            .FirstOrDefaultAsync(ct);
        if (fila is null) return null;

        var v = fila.Version;
        var version = new VersionDocumentoResponseDto(v.Id, v.NumeroVersion, v.Estado, v.Contenido, v.CreadaEn,
            v.CreadaPor.NombreCompleto, v.AprobadaEn, v.AprobadaPor?.NombreCompleto);
        return new DocumentoDetalleResponseDto(fila.Id, fila.Titulo, fila.TipoDocumento, fila.PlantillaTitulo, fila.CreadoEn, version, []);
    }

    /// <summary>
    /// El documento con una versión concreta (HU-073); nulo si el documento o la versión no existen en la empresa.
    /// <see cref="DocumentoDetalleResponseDto.PorCompletar"/> va vacío: lo calcula el servicio desde el contenido.
    /// </summary>
    internal static async Task<DocumentoDetalleResponseDto?> DetalleDeVersionAsync(
        NorviaDbContext db, Guid documentoId, Guid versionId, CancellationToken ct)
    {
        var fila = await db.VersionDocumento.AsNoTracking()
            .Where(v => v.DocumentoId == documentoId && v.Id == versionId)
            .Select(v => new
            {
                DocumentoId = v.Documento.Id,
                v.Documento.Titulo,
                v.Documento.TipoDocumento,
                v.Documento.CreadoEn,
                PlantillaTitulo = v.Documento.PlantillaDocumento != null ? v.Documento.PlantillaDocumento.Titulo : null,
                v.Id,
                v.NumeroVersion,
                v.Estado,
                v.Contenido,
                v.CreadaEn,
                CreadaPor = v.CreadaPor.Persona,
                v.AprobadaEn,
                AprobadaPor = v.AprobadaPor != null ? v.AprobadaPor.Persona : null
            })
            .FirstOrDefaultAsync(ct);
        if (fila is null) return null;

        var version = new VersionDocumentoResponseDto(fila.Id, fila.NumeroVersion, fila.Estado, fila.Contenido,
            fila.CreadaEn, fila.CreadaPor.NombreCompleto, fila.AprobadaEn, fila.AprobadaPor?.NombreCompleto);
        return new DocumentoDetalleResponseDto(fila.DocumentoId, fila.Titulo, fila.TipoDocumento, fila.PlantillaTitulo,
            fila.CreadoEn, version, []);
    }
}
