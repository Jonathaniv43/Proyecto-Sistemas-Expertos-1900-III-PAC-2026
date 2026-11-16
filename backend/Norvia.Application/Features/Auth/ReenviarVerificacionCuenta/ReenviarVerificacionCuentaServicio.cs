using Microsoft.Extensions.Options;
using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Correo;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Auth.ReenviarVerificacionCuenta.DTOs;

namespace Norvia.Application.Features.Auth.ReenviarVerificacionCuenta;

/// <summary>
/// Reenvía instrucciones de verificación. Siempre responde igual (no enumera cuentas).
/// No rota la contraseña temporal (solo está el hash en BD); el correo recuerda usar la del alta.
/// </summary>
public sealed class ReenviarVerificacionCuentaServicio(
    IReenviarVerificacionCuentaRepositorio repositorio,
    IPlantillasCorreo plantillas,
    IEnvioCorreo envioCorreo,
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
            var urlBase = correoOpciones.Value.UrlBaseFrontend.TrimEnd('/');
            var variables = new Dictionary<string, string>
            {
                ["Email"] = acceso.Email,
                ["UrlVerificacion"] = $"{urlBase}/verificar-cuenta"
            };

            var renderizado = await plantillas.RenderizarAsync(
                CodigosPlantillaCorreo.ReenvioVerificacion, variables, cancellationToken);
            if (renderizado is not null)
            {
                await envioCorreo.EnviarAsync(
                    new MensajeCorreo(acceso.Email, renderizado.Asunto, renderizado.CuerpoHtml),
                    cancellationToken);
            }
        }

        return RespuestaGenerica;
    }
}
