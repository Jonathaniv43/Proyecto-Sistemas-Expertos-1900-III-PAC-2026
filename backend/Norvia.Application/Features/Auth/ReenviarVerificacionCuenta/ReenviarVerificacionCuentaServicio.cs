using Microsoft.Extensions.Options;
using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Correo;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Common.Seguridad;
using Norvia.Application.Features.Auth.ReenviarVerificacionCuenta.DTOs;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Auth.ReenviarVerificacionCuenta;

/// <summary>
/// Reenvía el código de activación con un código nuevo (mismo mecanismo que el código de Microsoft, RN-86).
/// Siempre responde igual (no enumera cuentas). La ventana anti-spam (RN-86) limita cuántas veces se puede pedir.
/// </summary>
public sealed class ReenviarVerificacionCuentaServicio(
    IReenviarVerificacionCuentaRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IHashCodigoVerificacion hashCodigo,
    IPlantillasCorreo plantillas,
    IEnvioCorreo envioCorreo,
    IReloj reloj,
    IOptions<CorreoOpciones> correoOpciones) : IServicioCasoDeUso
{
    private static readonly ReenviarVerificacionCuentaResponseDto RespuestaGenerica = new(
        "Si el correo está registrado y pendiente de verificación, recibirá instrucciones.");

    public async Task<Resultado<ReenviarVerificacionCuentaResponseDto>> EjecutarAsync(
        ReenviarVerificacionCuentaRequestDto request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var acceso = await repositorio.BuscarAccesoPorEmailAsync(email, cancellationToken);

        if (acceso is not null && !acceso.Verificado)
        {
            var ahora = await reloj.AhoraAsync(cancellationToken);
            var verificacion = await repositorio.BuscarVerificacionAsync(acceso.Id, cancellationToken);
            if (verificacion is null)
            {
                verificacion = new VerificacionCorreo
                {
                    Proposito = PropositoVerificacionCorreo.ActivacionCuenta,
                    Email = acceso.Email,
                    AccesoId = acceso.Id
                };
                repositorio.AgregarVerificacion(verificacion);
            }

            if (VentanaEnvioCodigo.PuedeEnviar(verificacion, ahora))
            {
                var codigo = hashCodigo.Generar();
                var urlBase = correoOpciones.Value.UrlBaseFrontend.TrimEnd('/');
                var variables = new Dictionary<string, string>
                {
                    ["Email"] = acceso.Email,
                    ["Codigo"] = codigo,
                    ["Vigencia"] = VentanaEnvioCodigo.TextoVigenciaCorta,
                    ["UrlActivacion"] = $"{urlBase}/verificar-cuenta?correo={Uri.EscapeDataString(acceso.Email)}"
                };

                var renderizado = await plantillas.RenderizarAsync(
                    CodigosPlantillaCorreo.ActivacionCuenta, variables, cancellationToken);
                if (renderizado is not null)
                {
                    var envio = await envioCorreo.EnviarAsync(
                        new MensajeCorreo(acceso.Email, renderizado.Asunto, renderizado.CuerpoHtml), cancellationToken);
                    if (envio.Enviado)
                    {
                        // Solo se guarda tras el envío correcto: un correo fallido no gasta un intento de la ventana.
                        verificacion.CodigoHash = hashCodigo.Hashear(codigo);
                        verificacion.Intentos = 0;
                        verificacion.ExpiraEn = ahora.AddMinutes(VentanaEnvioCodigo.MinutosVigencia);
                    }
                }
            }

            await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        }

        return RespuestaGenerica;
    }
}
