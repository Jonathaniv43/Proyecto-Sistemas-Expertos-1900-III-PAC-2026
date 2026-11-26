using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Application.Features.Documentos.ObtenerDocumento;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Documentos;

public sealed class ObtenerDocumentoRepositorio(NorviaDbContext db) : IObtenerDocumentoRepositorio
{
    public Task<DocumentoDetalleResponseDto?> DetalleAsync(Guid id, CancellationToken ct) => ProyeccionDocumento.DetalleAsync(db, id, ct);
}
