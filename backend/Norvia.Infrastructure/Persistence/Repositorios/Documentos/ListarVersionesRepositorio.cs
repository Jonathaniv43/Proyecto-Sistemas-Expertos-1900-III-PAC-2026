using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Application.Features.Documentos.ListarVersiones;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Documentos;

// Documento, VersionDocumento y VinculoEmpresa son tablas de empresa: el filtro global limita a la sesión.
public sealed class ListarVersionesRepositorio(NorviaDbContext db) : IListarVersionesRepositorio
{
    public Task<bool> ExisteDocumentoAsync(Guid documentoId, CancellationToken ct) =>
        db.Documento.AnyAsync(d => d.Id == documentoId, ct);

    public async Task<IReadOnlyList<VersionResumenResponseDto>> ListarAsync(Guid documentoId, CancellationToken ct)
    {
        var filas = await db.VersionDocumento.AsNoTracking()
            .Where(v => v.DocumentoId == documentoId)
            .OrderByDescending(v => v.NumeroVersion)
            .Select(v => new
            {
                v.Id,
                v.NumeroVersion,
                v.Estado,
                v.CreadaEn,
                CreadaPor = v.CreadaPor.Persona,
                v.AprobadaEn,
                AprobadaPor = v.AprobadaPor != null ? v.AprobadaPor.Persona : null
            })
            .ToListAsync(ct);

        return filas
            .Select(v => new VersionResumenResponseDto(
                v.Id,
                v.NumeroVersion,
                v.Estado,
                v.CreadaEn,
                v.CreadaPor.NombreCompleto,
                v.AprobadaEn,
                v.AprobadaPor?.NombreCompleto))
            .ToList();
    }
}
