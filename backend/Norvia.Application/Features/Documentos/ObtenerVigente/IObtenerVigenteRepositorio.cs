using Norvia.Application.Features.Documentos.DTOs;

namespace Norvia.Application.Features.Documentos.ObtenerVigente;

public interface IObtenerVigenteRepositorio
{
    /// <summary>La versión vigente del documento; nula si no existe en la empresa o todavía no tiene una vigente.</summary>
    Task<DocumentoVigenteResponseDto?> VigenteAsync(Guid documentoId, CancellationToken ct);
}
