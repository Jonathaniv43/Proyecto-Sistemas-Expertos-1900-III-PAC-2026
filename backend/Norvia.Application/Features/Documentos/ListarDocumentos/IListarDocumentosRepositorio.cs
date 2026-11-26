using Norvia.Application.Features.Documentos.DTOs;

namespace Norvia.Application.Features.Documentos.ListarDocumentos;

public interface IListarDocumentosRepositorio
{
    /// <summary>Los documentos de la empresa de la sesión, el último actualizado primero.</summary>
    Task<IReadOnlyList<DocumentoResumenResponseDto>> ListarAsync(CancellationToken ct);
}
