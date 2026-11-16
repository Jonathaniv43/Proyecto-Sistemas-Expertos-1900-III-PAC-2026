using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Auth.VerificarCuenta.DTOs;

namespace Norvia.Application.Features.Auth.VerificarCuenta;

/// <summary>
/// Verifica la cuenta con correo + contraseña temporal del alta y establece la contraseña definitiva.
/// Sin JWT (RN-67): el Acceso aún no está verificado.
/// </summary>
public sealed class VerificarCuentaServicio(
    IVerificarCuentaRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IHasherPassword hasher) : IServicioCasoDeUso
{
    public async Task<Resultado> EjecutarAsync(VerificarCuentaRequestDto request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var acceso = await repositorio.BuscarAccesoPorEmailAsync(email, cancellationToken);
        if (acceso is null) return Errores.CredencialesInvalidas;

        if (acceso.Verificado) return Errores.CuentaYaVerificada;

        if (!hasher.Verificar(acceso, request.ContrasenaTemporal)) return Errores.CredencialesInvalidas;

        acceso.PasswordHash = hasher.Hashear(acceso, request.ContrasenaNueva);
        acceso.Verificado = true;
        acceso.DebeCambiarPassword = false;
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return Resultado.Exito();
    }
}
