using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Documentos.CrearVersionNueva;
using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Domain.Entities.Documentos;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Documentos;

// Documento y VersionDocumento son tablas de empresa: el filtro global limita a la sesión.
public sealed class CrearVersionNuevaRepositorio(NorviaDbContext db) : ICrearVersionNuevaRepositorio
{
    public Task<bool> ExisteDocumentoAsync(Guid documentoId, CancellationToken ct) =>
        db.Documento.AnyAsync(d => d.Id == documentoId, ct);

    public Task<bool> TieneBorradorAsync(Guid documentoId, CancellationToken ct) =>
        db.VersionDocumento.AnyAsync(v => v.DocumentoId == documentoId && v.Estado == EstadoVersionDocumento.Borrador, ct);

    public Task<VersionDocumento?> VigenteAsync(Guid documentoId, CancellationToken ct) =>
        db.VersionDocumento.AsNoTracking()
            .FirstOrDefaultAsync(v => v.DocumentoId == documentoId && v.Estado == EstadoVersionDocumento.Vigente, ct);

    public async Task<int> MaximoNumeroVersionAsync(Guid documentoId, CancellationToken ct) =>
        await db.VersionDocumento.Where(v => v.DocumentoId == documentoId)
            .MaxAsync(v => (int?)v.NumeroVersion, ct) ?? 0;

    public void Agregar(VersionDocumento version) => db.VersionDocumento.Add(version);

    public async Task<DocumentoDetalleResponseDto> DetalleAsync(Guid documentoId, CancellationToken ct) =>
        (await ProyeccionDocumento.DetalleAsync(db, documentoId, ct))!;
}
