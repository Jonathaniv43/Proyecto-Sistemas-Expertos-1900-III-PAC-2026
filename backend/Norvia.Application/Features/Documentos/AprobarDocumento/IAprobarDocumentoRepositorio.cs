using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Domain.Entities.Documentos;

namespace Norvia.Application.Features.Documentos.AprobarDocumento;

public interface IAprobarDocumentoRepositorio
{
    Task<bool> ExisteDocumentoAsync(Guid documentoId, CancellationToken ct);

    /// <summary>La versión en borrador con su documento, con seguimiento de cambios; nula si no tiene.</summary>
    Task<VersionDocumento?> BorradorAsync(Guid documentoId, CancellationToken ct);

    /// <summary>La versión vigente del documento, con seguimiento de cambios; nula si todavía no tiene.</summary>
    Task<VersionDocumento?> VigenteAsync(Guid documentoId, CancellationToken ct);

    /// <summary>Los vínculos activos de la empresa de la sesión (RN-75).</summary>
    Task<IReadOnlyList<Guid>> VinculosActivosAsync(CancellationToken ct);

    Task<DocumentoDetalleResponseDto> DetalleAsync(Guid documentoId, CancellationToken ct);
}
