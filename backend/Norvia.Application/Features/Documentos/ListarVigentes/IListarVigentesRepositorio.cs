using Norvia.Application.Features.Documentos.DTOs;

namespace Norvia.Application.Features.Documentos.ListarVigentes;

public interface IListarVigentesRepositorio
{
    /// <summary>Los documentos con versión vigente, por tipo y título; <paramref name="buscar"/> filtra por título.</summary>
    Task<IReadOnlyList<DocumentoVigenteResumenResponseDto>> ListarAsync(string? buscar, CancellationToken ct);
}
