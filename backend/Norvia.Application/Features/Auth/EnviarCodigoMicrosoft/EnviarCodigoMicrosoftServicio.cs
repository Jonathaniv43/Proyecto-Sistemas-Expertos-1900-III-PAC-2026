using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Correo;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Common.Seguridad;
using Norvia.Application.Features.Auth.EnviarCodigoMicrosoft.DTOs;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Auth.EnviarCodigoMicrosoft;

/// <summary>
/// Envía al correo de la cuenta de Microsoft el código de 6 dígitos que prueba que es de quien se registra (RN-86).
/// Solo aplica cuando ese correo todavía no tiene cuenta en Norvia: es el único caso en que se crea una cuenta
/// confiando en el correo que dice el proveedor. La fila de <see cref="VerificacionCorreo"/> guarda el
/// código (con hash), su vencimiento y los envíos de la ventana actual; no se borra sola, VerificarCodigoMicrosoft
/// la elimina al confirmar.
/// </summary>
public sealed class EnviarCodigoMicrosoftServicio(
    IEnviarCodigoMicrosoftRepositorio repo,
    IUnidadDeTrabajo uow,
    ITokenIdentidadExterna tokensIdentidad,
    IHashCodigoVerificacion hashCodigo,
    IPlantillasCorreo plantillas,
    IEnvioCorreo envioCorreo,
    IReloj reloj) : IServicioCasoDeUso
{
    public const int MinutosVigencia = 10;

    public async Task<Resultado<EnviarCodigoMicrosoftResponseDto>> EjecutarAsync(
        EnviarCodigoMicrosoftRequestDto request, CancellationToken cancellationToken)
    {
        var ahora = await reloj.AhoraAsync(cancellationToken);
        var identidad = tokensIdentidad.Leer(request.TokenIdentidad, ahora);
        if (identidad is null) return Errores.IdentidadInvalida;

        if (await repo.ProveedorVinculadoAsync(identidad.Proveedor, identidad.ProveedorUserId, cancellationToken)
            || await repo.ExisteEmailAsync(identidad.Email, cancellationToken))
            return Errores.CodigoNoRequerido;

        var fila = await repo.BuscarAsync(identidad.Email, identidad.ProveedorUserId, cancellationToken);
        if (fila is null)
        {
            fila = new VerificacionCorreo
            {
                Proposito = PropositoVerificacionCorreo.Microsoft,
                Email = identidad.Email,
                ProveedorUserId = identidad.ProveedorUserId
            };
            repo.Agregar(fila);
        }

        if (!VentanaEnvioCodigo.PuedeEnviar(fila, ahora)) return Errores.DemasiadosIntentos;

        var codigo = hashCodigo.Generar();
        var renderizado = await plantillas.RenderizarAsync(
            CodigosPlantillaCorreo.CodigoVerificacionCorreo,
            new Dictionary<string, string>
            {
                ["Email"] = identidad.Email,
                ["Codigo"] = codigo,
                ["Minutos"] = MinutosVigencia.ToString()
            },
            cancellationToken);
        if (renderizado is null) return Errores.CorreoNoEnviado;

        var envio = await envioCorreo.EnviarAsync(
            new MensajeCorreo(identidad.Email, renderizado.Asunto, renderizado.CuerpoHtml), cancellationToken);
        if (!envio.Enviado) return Errores.CorreoNoEnviado;

        // Solo se guarda tras el envío correcto: un correo fallido no gasta un intento de la ventana.
        fila.CodigoHash = hashCodigo.Hashear(codigo);
        fila.Intentos = 0;
        fila.ExpiraEn = ahora.AddMinutes(MinutosVigencia);
        await uow.GuardarCambiosAsync(cancellationToken);

        return new EnviarCodigoMicrosoftResponseDto($"Enviamos un código a {identidad.Email}.", MinutosVigencia);
    }
}
