using Norvia.Application.Features.Documentos.DTOs;

namespace Norvia.Application.Features.Documentos.ObtenerVersion;

public interface IObtenerVersionRepositorio
{
    Task<bool> ExisteDocumentoAsync(Guid documentoId, CancellationToken ct);
    Task<DocumentoDetalleResponseDto?> DetalleDeVersionAsync(Guid documentoId, Guid versionId, CancellationToken ct);
}
