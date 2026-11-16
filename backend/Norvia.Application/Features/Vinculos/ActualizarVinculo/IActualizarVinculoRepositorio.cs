using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Vinculos.ActualizarVinculo;

public interface IActualizarVinculoRepositorio
{
    /// <summary>Vínculo de la empresa de la sesión, con persona, acceso y rol.</summary>
    Task<VinculoEmpresa?> BuscarAsync(Guid id, CancellationToken ct);
    Task<Rol?> BuscarRolAsync(string codigo, CancellationToken ct);

    /// <summary>Administradores activos de la empresa de la sesión, sin contar <paramref name="excepto"/>.</summary>
    Task<int> ContarOtrosAdministradoresActivosAsync(Guid excepto, CancellationToken ct);

    void AgregarHistorial(HistorialEstadoVinculo historial);

    /// <summary>Cierra las sesiones abiertas con ese vínculo (solo de esta empresa).</summary>
    Task EliminarRefreshDelVinculoAsync(Guid vinculoId, CancellationToken ct);
}
