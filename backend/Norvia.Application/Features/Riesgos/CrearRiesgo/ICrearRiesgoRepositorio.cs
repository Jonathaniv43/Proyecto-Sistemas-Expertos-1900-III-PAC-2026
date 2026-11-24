using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.CrearRiesgo;

public interface ICrearRiesgoRepositorio
{
    /// <summary>Nombre y clasificación del peligro si existe y está activo; null si no.</summary>
    Task<(string Nombre, ClasificacionPeligro Clasificacion)?> PeligroActivoAsync(Guid peligroId, CancellationToken ct);

    /// <summary>
    /// true si la matriz de la empresa ya tiene ese peligro (del catálogo o descrito) con esa actividad,
    /// sin distinguir mayúsculas.
    /// </summary>
    Task<bool> ExisteEnMatrizAsync(Guid? peligroId, string? descripcionPeligro, string actividad, CancellationToken ct);

    /// <summary>true si el evento es de la empresa de la sesión y no está descartado (HU-051).</summary>
    Task<bool> EventoEnlazableAsync(Guid accidenteId, CancellationToken ct);

    void Agregar(RiesgoEmpresa riesgo);
}
