using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Vinculos;

/// <summary>Datos que necesita <see cref="AltaDeVinculo"/>. Compartido por los slices de alta del módulo.</summary>
public interface IAltaDeVinculoRepositorio
{
    Task<Acceso?> BuscarAccesoAsync(string emailNormalizado, CancellationToken ct);

    /// <summary>Persona que ya usa ese Acceso en cualquier empresa; nula si el Acceso no tiene vínculos (p. ej. staff).</summary>
    Task<Persona?> BuscarPersonaDelAccesoAsync(Guid accesoId, CancellationToken ct);

    /// <summary>Si el Acceso ya tiene un vínculo, en cualquier estado, con la empresa de la sesión.</summary>
    Task<bool> TieneVinculoEnEmpresaActualAsync(Guid accesoId, CancellationToken ct);

    /// <summary>Razón social de la empresa, para el texto del correo de activación.</summary>
    Task<string> BuscarNombreEmpresaAsync(Guid empresaId, CancellationToken ct);

    void AgregarPersona(Persona persona, Usuario usuario);
    void AgregarAcceso(Acceso acceso);
    void AgregarVinculo(VinculoEmpresa vinculo);
    void AgregarVerificacionCorreo(VerificacionCorreo fila);
}
