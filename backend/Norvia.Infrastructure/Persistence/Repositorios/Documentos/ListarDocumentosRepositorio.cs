using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Application.Features.Documentos.ListarDocumentos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Documentos;

public sealed class ListarDocumentosRepositorio(NorviaDbContext db) : IListarDocumentosRepositorio
{
    public async Task<IReadOnlyList<DocumentoResumenResponseDto>> ListarAsync(CancellationToken ct)
    {
        var filas = await db.Documento.AsNoTracking()
            .Select(d => new
            {
                d.Id, d.Titulo, d.TipoDocumento,
                Version = d.Versiones.AsQueryable().OrderBy(ProyeccionDocumento.PrioridadDeTrabajo).ThenByDescending(v => v.NumeroVersion)
                    .Select(v => new { v.NumeroVersion, v.Estado, Fecha = v.AprobadaEn ?? v.CreadaEn })
                    .First()
            })
            .ToListAsync(ct);

        return filas
            .Select(f => new DocumentoResumenResponseDto(f.Id, f.Titulo, f.TipoDocumento, f.Version.NumeroVersion, f.Version.Estado, f.Version.Fecha))
            .OrderByDescending(d => d.ActualizadoEn).ThenBy(d => d.Titulo, StringComparer.CurrentCulture).ThenBy(d => d.Id)
            .ToList();
    }
}
