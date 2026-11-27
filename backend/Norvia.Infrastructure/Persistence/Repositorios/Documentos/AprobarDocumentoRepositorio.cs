using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Documentos.AprobarDocumento;
using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Domain.Entities.Documentos;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Documentos;

// Documento, VersionDocumento y VinculoEmpresa son tablas de empresa: el filtro global limita a la sesión.
public sealed class AprobarDocumentoRepositorio(NorviaDbContext db) : IAprobarDocumentoRepositorio
{
    public Task<bool> ExisteDocumentoAsync(Guid documentoId, CancellationToken ct) =>
        db.Documento.AnyAsync(d => d.Id == documentoId, ct);

    public Task<VersionDocumento?> BorradorAsync(Guid documentoId, CancellationToken ct) =>
        db.VersionDocumento.Include(v => v.Documento)
            .FirstOrDefaultAsync(v => v.DocumentoId == documentoId && v.Estado == EstadoVersionDocumento.Borrador, ct);

    public Task<VersionDocumento?> VigenteAsync(Guid documentoId, CancellationToken ct) =>
        db.VersionDocumento.FirstOrDefaultAsync(v => v.DocumentoId == documentoId && v.Estado == EstadoVersionDocumento.Vigente, ct);

    public async Task<IReadOnlyList<Guid>> VinculosActivosAsync(CancellationToken ct) =>
        await db.VinculoEmpresa.Where(v => v.Estado == EstadoVinculo.Activo).Select(v => v.Id).ToListAsync(ct);

    public async Task<DocumentoDetalleResponseDto> DetalleAsync(Guid documentoId, CancellationToken ct) =>
        (await ProyeccionDocumento.DetalleAsync(db, documentoId, ct))!;
}
