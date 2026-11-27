using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Application.Features.Documentos.ObtenerVersion;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Documentos;

public sealed class ObtenerVersionRepositorio(NorviaDbContext db) : IObtenerVersionRepositorio
{
    public Task<bool> ExisteDocumentoAsync(Guid documentoId, CancellationToken ct) =>
        db.Documento.AnyAsync(d => d.Id == documentoId, ct);

    public Task<DocumentoDetalleResponseDto?> DetalleDeVersionAsync(Guid documentoId, Guid versionId, CancellationToken ct) =>
        ProyeccionDocumento.DetalleDeVersionAsync(db, documentoId, versionId, ct);
}
