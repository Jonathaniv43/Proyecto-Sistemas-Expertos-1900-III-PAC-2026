using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Vinculos.CrearVinculo;

public interface ICrearVinculoRepositorio
{
    Task<Rol?> BuscarRolAsync(string codigo, CancellationToken ct);
    Task<Acceso?> BuscarAccesoAsync(string emailNormalizado, CancellationToken ct);

    /// <summary>Persona que ya usa ese Acceso en cualquier empresa; nula si el Acceso no tiene vínculos (p. ej. staff).</summary>
    Task<Persona?> BuscarPersonaDelAccesoAsync(Guid accesoId, CancellationToken ct);

    /// <summary>Si el Acceso ya tiene un vínculo, en cualquier estado, con la empresa de la sesión.</summary>
    Task<bool> TieneVinculoEnEmpresaActualAsync(Guid accesoId, CancellationToken ct);

    void AgregarPersona(Persona persona, Usuario usuario);
    void AgregarAcceso(Acceso acceso);
    void AgregarVinculo(VinculoEmpresa vinculo);
}
