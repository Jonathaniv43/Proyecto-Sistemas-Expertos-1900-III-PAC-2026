using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Auth.ConfirmarCambioContrasena.DTOs;

namespace Norvia.Application.Features.Auth.ConfirmarCambioContrasena;

/// <summary>
/// HU-022: cambia la contraseña con el enlace del correo. El enlace es válido si su firma es correcta,
/// no venció y la contraseña no cambió desde que se pidió (sirve una sola vez); si no, 410.
/// La contraseña es del Acceso (RN-65): vale en todas sus empresas y se cierran todas sus sesiones (HU-114).
/// </summary>
public sealed class ConfirmarCambioContrasenaServicio(
    IConfirmarCambioContrasenaRepositorio repositorio,
    ITokenCambioContrasena tokens,
    IHasherPassword hasher,
    IReloj reloj,
    IUnidadDeTrabajo unidadDeTrabajo) : IServicioCasoDeUso
{
    public async Task<Resultado> EjecutarAsync(ConfirmarCambioContrasenaRequestDto request, CancellationToken cancellationToken)
    {
        var enlace = tokens.Leer(request.Token.Trim());
        if (enlace is null) return Errores.EnlaceCambioContrasenaInvalido;

        var acceso = await repositorio.BuscarAccesoAsync(enlace.AccesoId, cancellationToken);
        var ahora = await reloj.AhoraAsync(cancellationToken);
        if (acceso?.PasswordHash is null || enlace.ExpiraUtc <= ahora || !tokens.SelloVigente(enlace, acceso.PasswordHash))
            return Errores.EnlaceCambioContrasenaInvalido;

        if (hasher.Verificar(acceso, request.ContrasenaNueva)) return Errores.ContrasenaIgualActual;

        acceso.PasswordHash = hasher.Hashear(acceso, request.ContrasenaNueva);
        acceso.DebeCambiarPassword = false;
        await repositorio.EliminarRefreshDelAccesoAsync(acceso.Id, cancellationToken);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Resultado.Exito();
    }
}
