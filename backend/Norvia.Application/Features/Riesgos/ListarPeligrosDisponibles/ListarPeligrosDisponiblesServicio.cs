using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Riesgos.ListarPeligrosDisponibles.DTOs;

namespace Norvia.Application.Features.Riesgos.ListarPeligrosDisponibles;

/// <summary>
/// HU-036: el catálogo de peligros que una empresa puede elegir. Es el mismo para todas (D9), pero
/// <c>api/peligros</c> es del staff de plataforma; esta consulta es de sesión de empresa y solo trae los activos.
/// </summary>
public sealed class ListarPeligrosDisponiblesServicio(IListarPeligrosDisponiblesRepositorio repositorio) : IServicioCasoDeUso
{
    public Task<IReadOnlyList<PeligroDisponibleResponseDto>> EjecutarAsync(CancellationToken ct) => repositorio.ListarAsync(ct);
}
