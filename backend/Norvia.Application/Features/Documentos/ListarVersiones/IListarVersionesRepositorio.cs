using Norvia.Application.Features.Documentos.DTOs;

namespace Norvia.Application.Features.Documentos.ListarVersiones;

public interface IListarVersionesRepositorio
{
    Task<bool> ExisteDocumentoAsync(Guid documentoId, CancellationToken ct);
    Task<IReadOnlyList<VersionResumenResponseDto>> ListarAsync(Guid documentoId, CancellationToken ct);
}
