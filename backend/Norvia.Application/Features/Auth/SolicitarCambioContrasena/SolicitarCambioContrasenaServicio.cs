using Microsoft.Extensions.Options;
using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Correo;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Auth.SolicitarCambioContrasena.DTOs;

namespace Norvia.Application.Features.Auth.SolicitarCambioContrasena;

/// <summary>
/// HU-022: envía al correo de la cuenta un enlace para cambiar la contraseña. Así se confirma que la
/// persona puede acceder a ese correo antes de permitir el cambio.
/// <list type="bullet">
/// <item>Siempre responde igual, exista o no la cuenta (no enumera correos).</item>
/// <item>Solo se envía a cuentas verificadas: una sin verificar se activa con su contraseña temporal (RN-67).</item>
/// <item>El enlace vence en <see cref="MinutosVigencia"/> minutos y sirve una sola vez (ver <see cref="ITokenCambioContrasena"/>).</item>
/// </list>
/// </summary>
public sealed class SolicitarCambioContrasenaServicio(
    ISolicitarCambioContrasenaRepositorio repositorio,
    ITokenCambioContrasena tokens,
    IReloj reloj,
    IPlantillasCorreo plantillas,
    IEnvioCorreo envioCorreo,
    IOptions<CorreoOpciones> correoOpciones) : IServicioCasoDeUso
{
    public const int MinutosVigencia = 30;

    private static readonly SolicitarCambioContrasenaResponseDto RespuestaGenerica = new(
        "Si el correo está registrado, recibirá un enlace para cambiar la contraseña.");

    public async Task<Resultado<SolicitarCambioContrasenaResponseDto>> EjecutarAsync(
        SolicitarCambioContrasenaRequestDto request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var acceso = await repositorio.BuscarAccesoPorEmailAsync(email, cancellationToken);
        if (acceso is null || !acceso.Verificado) return RespuestaGenerica;

        var ahora = await reloj.AhoraAsync(cancellationToken);
        var token = tokens.Crear(acceso.Id, acceso.PasswordHash, ahora.AddMinutes(MinutosVigencia));
        var urlBase = correoOpciones.Value.UrlBaseFrontend.TrimEnd('/');
        var variables = new Dictionary<string, string>
        {
            ["Nombre"] = await repositorio.BuscarPrimerNombreAsync(acceso.Id, cancellationToken) ?? acceso.Email,
            ["UrlCambio"] = $"{urlBase}/restablecer-contrasena?token={Uri.EscapeDataString(token)}",
            ["Minutos"] = MinutosVigencia.ToString()
        };
        var renderizado = await plantillas.RenderizarAsync(CodigosPlantillaCorreo.CambioContrasena, variables, cancellationToken);
        if (renderizado is not null)
            await envioCorreo.EnviarAsync(new MensajeCorreo(acceso.Email, renderizado.Asunto, renderizado.CuerpoHtml), cancellationToken);

        return RespuestaGenerica;
    }
}
