using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Auth.ReestablecerContrasena.DTOs;

namespace Norvia.Application.Features.Auth.ReestablecerContrasena;

/// <summary>
/// Cambia la contraseña del Acceso de la sesión, cumpliendo la política de HU-114. Sirve también para
/// reemplazar una contraseña temporal en el primer ingreso (<c>DebeCambiarPassword</c>).
/// El Acceso es único por correo (RN-65), así que la nueva contraseña vale en todas sus empresas;
/// por eso se cierran todas sus sesiones y hay que volver a iniciar sesión.
/// </summary>
public sealed class ReestablecerContrasenaServicio(
    IReestablecerContrasenaRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IHasherPassword hasher,
    ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado> EjecutarAsync(ReestablecerContrasenaRequestDto request, CancellationToken cancellationToken)
    {
        if (sesion.AccesoId is not Guid accesoId) return Errores.SesionInvalida;

        var acceso = await repositorio.BuscarAccesoAsync(accesoId, cancellationToken);
        if (acceso is null) return Errores.SesionInvalida;
        if (!hasher.Verificar(acceso, request.ContrasenaActual)) return Errores.ContrasenaActualIncorrecta;

        acceso.PasswordHash = hasher.Hashear(acceso, request.ContrasenaNueva);
        acceso.DebeCambiarPassword = false;
        await repositorio.EliminarRefreshDelAccesoAsync(accesoId, cancellationToken);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return Resultado.Exito();
    }
}