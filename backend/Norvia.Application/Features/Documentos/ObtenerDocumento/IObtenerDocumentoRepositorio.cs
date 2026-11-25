using Norvia.Application.Features.Documentos.DTOs;

namespace Norvia.Application.Features.Documentos.ObtenerDocumento;

public interface IObtenerDocumentoRepositorio
{
    /// <summary>El documento de la empresa de la sesión con su versión de trabajo; nulo si no existe.</summary>
    Task<DocumentoDetalleResponseDto?> DetalleAsync(Guid id, CancellationToken ct);
}
