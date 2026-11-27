using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Domain.Entities.Documentos;

namespace Norvia.Application.Features.Documentos.CrearVersionNueva;

public interface ICrearVersionNuevaRepositorio
{
    Task<bool> ExisteDocumentoAsync(Guid documentoId, CancellationToken ct);
    Task<bool> TieneBorradorAsync(Guid documentoId, CancellationToken ct);
    Task<VersionDocumento?> VigenteAsync(Guid documentoId, CancellationToken ct);
    Task<int> MaximoNumeroVersionAsync(Guid documentoId, CancellationToken ct);
    void Agregar(VersionDocumento version);
    Task<DocumentoDetalleResponseDto> DetalleAsync(Guid documentoId, CancellationToken ct);
}
