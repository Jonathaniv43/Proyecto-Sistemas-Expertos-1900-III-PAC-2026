using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Common.Seguridad;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Vinculos;

/// <summary>Datos de la persona tal como los envía el administrador.</summary>
public sealed record DatosPersonaAlta(
    string Email, string PrimerNombre, string? SegundoNombre, string PrimerApellido, string? SegundoApellido);

/// <param name="PasswordGenerada">La contraseña temporal generada por Norvia; nula si no se generó.</param>
public sealed record VinculoPreparado(
    VinculoEmpresa Vinculo, Persona Persona, Acceso Acceso, bool CuentaNueva, string? PasswordGenerada);

/// <summary>
/// Alta de una persona en la empresa de la sesión, compartida por los slices del módulo (HU-020 y HU-115):
/// crea la cuenta o reutiliza la existente (RN-02, RN-65) y prepara el vínculo. No guarda: el servicio
/// llama a la unidad de trabajo, así un lote completo se confirma en una sola transacción.
/// </summary>
public sealed class AltaDeVinculo(IAltaDeVinculoRepositorio repositorio, IHasherPassword hasher)
{
    /// <param name="passwordTemporal">Para una cuenta nueva; si es nula, depende de <paramref name="generarSiFalta"/>.</param>
    /// <param name="generarSiFalta">true: si la cuenta es nueva y no hay contraseña, Norvia genera una.</param>
    public async Task<Resultado<VinculoPreparado>> PrepararAsync(
        DatosPersonaAlta datos, short rolId, string? passwordTemporal, bool generarSiFalta, CancellationToken ct)
    {
        var email = datos.Email.Trim().ToLowerInvariant();
        var acceso = await repositorio.BuscarAccesoAsync(email, ct);
        var cuentaNueva = acceso is null;
        string? generada = null;
        Persona? persona = null;

        if (acceso is not null)
        {
            if (await repositorio.TieneVinculoEnEmpresaActualAsync(acceso.Id, ct)) return Errores.VinculoDuplicado;
            persona = await repositorio.BuscarPersonaDelAccesoAsync(acceso.Id, ct);
        }
        else
        {
            if (passwordTemporal is null)
            {
                if (!generarSiFalta) return Errores.PasswordTemporalRequerida;
                passwordTemporal = generada = GeneradorContrasenaTemporal.Generar();
            }

            acceso = new Acceso { Email = email, Verificado = true, DebeCambiarPassword = true };
            acceso.PasswordHash = hasher.Hashear(acceso, passwordTemporal);
            repositorio.AgregarAcceso(acceso);
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

        return new VinculoPreparado(vinculo, persona, acceso, cuentaNueva, generada);
    }

    private static string? Opcional(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
