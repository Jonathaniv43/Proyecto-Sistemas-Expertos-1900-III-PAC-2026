using Norvia.Domain.Entities.Riesgos;

namespace Norvia.Application.Features.Riesgos.CrearRiesgo;

public interface ICrearRiesgoRepositorio
{
    /// <summary>Nombre del peligro si existe y está activo; null si no.</summary>
    Task<string?> NombrePeligroActivoAsync(Guid peligroId, CancellationToken ct);

    /// <summary>
    /// true si la matriz de la empresa ya tiene ese peligro (del catálogo o descrito) con esa actividad,
    /// sin distinguir mayúsculas.
    /// </summary>
    Task<bool> ExisteEnMatrizAsync(Guid? peligroId, string? descripcionPeligro, string actividad, CancellationToken ct);

    void Agregar(RiesgoEmpresa riesgo);
}
