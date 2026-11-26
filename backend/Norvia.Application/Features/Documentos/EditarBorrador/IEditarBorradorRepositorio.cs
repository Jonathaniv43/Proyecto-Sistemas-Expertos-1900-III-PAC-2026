using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Domain.Entities.Documentos;

namespace Norvia.Application.Features.Documentos.EditarBorrador;

public interface IEditarBorradorRepositorio
{
    Task<bool> ExisteDocumentoAsync(Guid documentoId, CancellationToken ct);

    /// <summary>La versión en borrador del documento, con seguimiento de cambios; nula si no tiene.</summary>
    Task<VersionDocumento?> BorradorAsync(Guid documentoId, CancellationToken ct);

    Task<DocumentoDetalleResponseDto> DetalleAsync(Guid documentoId, CancellationToken ct);
}
