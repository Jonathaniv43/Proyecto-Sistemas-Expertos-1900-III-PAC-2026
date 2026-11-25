using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Application.Features.Documentos.EditarBorrador;
using Norvia.Domain.Entities.Documentos;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Documentos;

// Documento y VersionDocumento son tablas de empresa: el filtro global limita a la sesión.
public sealed class EditarBorradorRepositorio(NorviaDbContext db) : IEditarBorradorRepositorio
{
    public Task<bool> ExisteDocumentoAsync(Guid documentoId, CancellationToken ct) =>
        db.Documento.AnyAsync(d => d.Id == documentoId, ct);

    public Task<VersionDocumento?> BorradorAsync(Guid documentoId, CancellationToken ct) =>
        db.VersionDocumento.FirstOrDefaultAsync(v => v.DocumentoId == documentoId && v.Estado == EstadoVersionDocumento.Borrador, ct);

    public async Task<DocumentoDetalleResponseDto> DetalleAsync(Guid documentoId, CancellationToken ct) =>
        (await ProyeccionDocumento.DetalleAsync(db, documentoId, ct))!;
}
