using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Vinculos.CrearVinculo;

/// <summary>Lo propio del slice; la cuenta, la persona y el vínculo los prepara <see cref="AltaDeVinculo"/>.</summary>
public interface ICrearVinculoRepositorio
{
    Task<Rol?> BuscarRolAsync(string codigo, CancellationToken ct);
}
