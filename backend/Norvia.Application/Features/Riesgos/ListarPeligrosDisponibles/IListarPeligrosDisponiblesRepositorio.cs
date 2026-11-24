using Norvia.Application.Features.Riesgos.ListarPeligrosDisponibles.DTOs;

namespace Norvia.Application.Features.Riesgos.ListarPeligrosDisponibles;

public interface IListarPeligrosDisponiblesRepositorio
{
    /// <summary>Peligros activos del catálogo global, por nombre.</summary>
    Task<IReadOnlyList<PeligroDisponibleResponseDto>> ListarAsync(CancellationToken ct);
}
