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

/// <param name="CodigoActivacion">
/// Código en claro de una cuenta nueva, solo en memoria hasta enviarlo (en la base queda su hash). Nulo si la cuenta ya existía.
/// </param>
public sealed record VinculoPreparado(
    VinculoEmpresa Vinculo, Persona Persona, Acceso Acceso, string RolNombre, string? CodigoActivacion)
{
    public bool CuentaNueva => CodigoActivacion is not null;
}

/// <summary>
/// Alta de una persona en la empresa de la sesión, compartida por los slices del módulo (HU-020 y HU-115):
/// crea la cuenta o reutiliza la existente (RN-02, RN-65) y prepara el vínculo.
/// <para>
/// Una cuenta nueva no recibe contraseña de nadie: queda sin <c>PasswordHash</c> y se le manda un código de
/// 6 dígitos para activarla (RN-87). Un correo no se puede deshacer: primero el servicio guarda todo (con el hash
/// del código) y solo después llama a <see cref="EnviarActivacionAsync"/>. Si el envío falla, la cuenta ya existe
/// y la persona pide "Reenviar código".
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
    /// <summary>Deja todo pendiente en la unidad de trabajo; no guarda ni envía.</summary>
    public async Task<Resultado<VinculoPreparado>> PrepararAsync(
        DatosPersonaAlta datos, short rolId, string rolNombre, CancellationToken ct)
    {
        var email = datos.Email.Trim().ToLowerInvariant();
        var acceso = await repositorio.BuscarAccesoAsync(email, ct);
        Persona? persona = null;
        string? codigo = null;

        if (acceso is not null)
        {
            if (await repositorio.TieneVinculoEnEmpresaActualAsync(acceso.Id, ct)) return Errores.VinculoDuplicado;
            persona = await repositorio.BuscarPersonaDelAccesoAsync(acceso.Id, ct);
        }
        else
        {
            acceso = new Acceso { Email = email, Verificado = false };
            repositorio.AgregarAcceso(acceso);
            codigo = await PrepararCodigoAsync(acceso, ct);
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

        return new VinculoPreparado(vinculo, persona, acceso, rolNombre, codigo);
    }

    /// <summary>
    /// Envía el código de una cuenta nueva. Llamar solo después de guardar. true si no hacía falta (cuenta ya
    /// existente) o si se envió; false si la cuenta es nueva y el envío falló (puede reenviarse).
    /// </summary>
    public async Task<bool> EnviarActivacionAsync(VinculoPreparado preparado, CancellationToken ct)
    {
        if (preparado.CodigoActivacion is null) return true;

        var empresaNombre = sesion.EmpresaId is Guid empresaId
            ? await repositorio.BuscarNombreEmpresaAsync(empresaId, ct)
            : string.Empty;
        var email = preparado.Acceso.Email;
        var urlBase = correoOpciones.Value.UrlBaseFrontend.TrimEnd('/');
        var renderizado = await plantillas.RenderizarAsync(
            CodigosPlantillaCorreo.ActivacionCuenta,
            new Dictionary<string, string>
            {
                ["Email"] = email,
                ["EmpresaNombre"] = empresaNombre,
                ["RolNombre"] = preparado.RolNombre,
                ["Codigo"] = preparado.CodigoActivacion,
                ["Vigencia"] = VentanaEnvioCodigo.TextoVigenciaActivacionPorAlta,
                ["UrlActivacion"] = $"{urlBase}/verificar-cuenta?correo={Uri.EscapeDataString(email)}"
            },
            ct);
        if (renderizado is null) return false;

        var envio = await envioCorreo.EnviarAsync(new MensajeCorreo(email, renderizado.Asunto, renderizado.CuerpoHtml), ct);
        return envio.Enviado;
    }

    /// <summary>Genera el código y deja su hash en una fila nueva de VerificacionCorreo. Devuelve el código en claro.</summary>
    private async Task<string> PrepararCodigoAsync(Acceso acceso, CancellationToken ct)
    {
        var ahora = await reloj.AhoraAsync(ct);
        var codigo = hashCodigo.Generar();
        var verificacion = new VerificacionCorreo
        {
            Proposito = PropositoVerificacionCorreo.ActivacionCuenta,
            Email = acceso.Email,
            AccesoId = acceso.Id,
            CodigoHash = hashCodigo.Hashear(codigo),
            // Quien recibe el correo no lo espera (lo dio de alta otra persona): vigencia larga (RN-87).
            ExpiraEn = ahora + VentanaEnvioCodigo.VigenciaActivacionPorAlta
        };
        VentanaEnvioCodigo.PuedeEnviar(verificacion, ahora);
        repositorio.AgregarVerificacionCorreo(verificacion);
        return codigo;
    }

    private static string? Opcional(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
