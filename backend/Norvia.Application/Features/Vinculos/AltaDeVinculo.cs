using Microsoft.Extensions.Options;
using Norvia.Application.Common.Correo;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Common.Seguridad;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Vinculos;

/// <summary>Datos de la persona tal como los envía el administrador.</summary>
public sealed record DatosPersonaAlta(
    string Email, string PrimerNombre, string? SegundoNombre, string PrimerApellido, string? SegundoApellido);

/// <param name="CorreoActivacionEnviado">
/// true si no hacía falta (cuenta ya existente) o si el correo con el código de activación se envió; false si la
/// cuenta es nueva pero el envío falló (puede reenviarse, HU-115).
/// </param>
public sealed record VinculoPreparado(
    VinculoEmpresa Vinculo, Persona Persona, Acceso Acceso, bool CuentaNueva, bool CorreoActivacionEnviado);

/// <summary>
/// Alta de una persona en la empresa de la sesión, compartida por los slices del módulo (HU-020 y HU-115):
/// crea la cuenta o reutiliza la existente (RN-02, RN-65) y prepara el vínculo. No guarda: el servicio
/// llama a la unidad de trabajo, así un lote completo se confirma en una sola transacción.
/// <para>
/// Una cuenta nueva no recibe contraseña de nadie: queda sin <c>PasswordHash</c> y se le manda un código de
/// 6 dígitos para activarla (mismo mecanismo que el código de Microsoft, RN-86). Mientras no la active, la
/// cuenta no sirve para entrar.
/// </para>
/// </summary>
public sealed class AltaDeVinculo(
    IAltaDeVinculoRepositorio repositorio,
    IHashCodigoVerificacion hashCodigo,
    IPlantillasCorreo plantillas,
    IEnvioCorreo envioCorreo,
    IReloj reloj,
    IOptions<CorreoOpciones> correoOpciones,
    ITenantActual sesion)
{
    public async Task<Resultado<VinculoPreparado>> PrepararAsync(
        DatosPersonaAlta datos, short rolId, string rolNombre, CancellationToken ct)
    {
        var email = datos.Email.Trim().ToLowerInvariant();
        var acceso = await repositorio.BuscarAccesoAsync(email, ct);
        var cuentaNueva = acceso is null;
        Persona? persona = null;
        var correoActivacionEnviado = true;

        if (acceso is not null)
        {
            if (await repositorio.TieneVinculoEnEmpresaActualAsync(acceso.Id, ct)) return Errores.VinculoDuplicado;
            persona = await repositorio.BuscarPersonaDelAccesoAsync(acceso.Id, ct);
        }
        else
        {
            acceso = new Acceso { Email = email, Verificado = false };
            repositorio.AgregarAcceso(acceso);
            correoActivacionEnviado = await EnviarActivacionAsync(acceso, rolNombre, ct);
        }

        // Una cuenta sin persona (cuenta nueva, o una de staff sin vínculos) recibe una persona nueva.
        if (persona is null)
        {
            persona = new Persona
            {
                PrimerNombre = datos.PrimerNombre.Trim(),
                SegundoNombre = Opcional(datos.SegundoNombre),
                PrimerApellido = datos.PrimerApellido.Trim(),
                SegundoApellido = Opcional(datos.SegundoApellido)
            };
            repositorio.AgregarPersona(persona, new Usuario { PersonaId = persona.Id, Activo = true });
        }

        // EmpresaId lo pone el interceptor con la empresa del token (RN-06).
        var vinculo = new VinculoEmpresa
        {
            PersonaId = persona.Id,
            AccesoId = acceso.Id,
            RolId = rolId,
            Estado = EstadoVinculo.Activo
        };
        repositorio.AgregarVinculo(vinculo);

        return new VinculoPreparado(vinculo, persona, acceso, cuentaNueva, correoActivacionEnviado);
    }

    private async Task<bool> EnviarActivacionAsync(Acceso acceso, string rolNombre, CancellationToken ct)
    {
        var ahora = await reloj.AhoraAsync(ct);
        var verificacion = new VerificacionCorreo
        {
            Proposito = PropositoVerificacionCorreo.ActivacionCuenta,
            Email = acceso.Email,
            AccesoId = acceso.Id
        };
        VentanaEnvioCodigo.PuedeEnviar(verificacion, ahora);
        repositorio.AgregarVerificacionCorreo(verificacion);

        var empresaNombre = sesion.EmpresaId is Guid empresaId
            ? await repositorio.BuscarNombreEmpresaAsync(empresaId, ct)
            : string.Empty;

        var codigo = hashCodigo.Generar();
        var urlBase = correoOpciones.Value.UrlBaseFrontend.TrimEnd('/');
        var renderizado = await plantillas.RenderizarAsync(
            CodigosPlantillaCorreo.ActivacionCuenta,
            new Dictionary<string, string>
            {
                ["Email"] = acceso.Email,
                ["EmpresaNombre"] = empresaNombre,
                ["RolNombre"] = rolNombre,
                ["Codigo"] = codigo,
                ["Minutos"] = VentanaEnvioCodigo.MinutosVigencia.ToString(),
                ["UrlActivacion"] = $"{urlBase}/activar-cuenta"
            },
            ct);
        if (renderizado is null) return false;

        var envio = await envioCorreo.EnviarAsync(
            new MensajeCorreo(acceso.Email, renderizado.Asunto, renderizado.CuerpoHtml), ct);
        if (!envio.Enviado) return false;

        // Solo se guarda tras el envío correcto: un correo fallido no gasta un intento de la ventana.
        verificacion.CodigoHash = hashCodigo.Hashear(codigo);
        verificacion.ExpiraEn = ahora.AddMinutes(VentanaEnvioCodigo.MinutosVigencia);
        return true;
    }

    private static string? Opcional(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
