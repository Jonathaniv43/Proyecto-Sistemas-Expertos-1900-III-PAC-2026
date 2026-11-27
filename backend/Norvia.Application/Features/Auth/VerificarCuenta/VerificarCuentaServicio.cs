using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Auth.VerificarCuenta.DTOs;

namespace Norvia.Application.Features.Auth.VerificarCuenta;

/// <summary>
/// Verifica la cuenta con el código de 6 dígitos que se envió al activarla (mismo mecanismo que el código de
/// Microsoft, RN-86) y establece la contraseña definitiva. Sin JWT (RN-67): el Acceso aún no está verificado.
/// </summary>
public sealed class VerificarCuentaServicio(
    IVerificarCuentaRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IHashCodigoVerificacion hashCodigo,
    IHasherPassword hasher,
    IReloj reloj) : IServicioCasoDeUso
{
    private const int MaximoIntentos = 5;

    public async Task<Resultado> EjecutarAsync(VerificarCuentaRequestDto request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var acceso = await repositorio.BuscarAccesoPorEmailAsync(email, cancellationToken);
        if (acceso is null) return Errores.CredencialesInvalidas;
        if (acceso.Verificado) return Errores.CuentaYaVerificada;

        var ahora = await reloj.AhoraAsync(cancellationToken);
        var verificacion = await repositorio.BuscarVerificacionAsync(acceso.Id, cancellationToken);
        if (verificacion is null || verificacion.ExpiraEn <= ahora) return Errores.CodigoIncorrecto;
        if (verificacion.Intentos >= MaximoIntentos) return Errores.DemasiadosIntentos;

        if (!hashCodigo.Coincide(verificacion.CodigoHash, request.Codigo))
        {
            verificacion.Intentos++;
            await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
            return Errores.CodigoIncorrecto;
        }

        acceso.PasswordHash = hasher.Hashear(acceso, request.ContrasenaNueva);
        acceso.Verificado = true;
        repositorio.EliminarVerificacion(verificacion);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return Resultado.Exito();
    }
}
