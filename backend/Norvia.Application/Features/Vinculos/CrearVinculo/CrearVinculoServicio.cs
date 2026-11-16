using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Vinculos.CrearVinculo.DTOs;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Vinculos.CrearVinculo;

/// <summary>
/// Da acceso a la empresa de la sesión (HU-020, RN-02, D5): crea la cuenta o reutiliza la existente,
/// y crea el vínculo con el rol elegido. La empresa sale del token (RN-06): la pone el interceptor.
/// </summary>
public sealed class CrearVinculoServicio(
    ICrearVinculoRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IHasherPassword hasher) : IServicioCasoDeUso
{
    public async Task<Resultado<CrearVinculoResponseDto>> EjecutarAsync(CrearVinculoRequestDto request, CancellationToken ct)
    {
        var rol = await repositorio.BuscarRolAsync(request.RolCodigo.Trim(), ct);
        if (rol is null) return Errores.RolInvalido;

        var email = request.Email.Trim().ToLowerInvariant();
        var acceso = await repositorio.BuscarAccesoAsync(email, ct);
        var cuentaNueva = acceso is null;
        Persona? persona = null;

        if (acceso is not null)
        {
            if (await repositorio.TieneVinculoEnEmpresaActualAsync(acceso.Id, ct)) return Errores.VinculoDuplicado;
            persona = await repositorio.BuscarPersonaDelAccesoAsync(acceso.Id, ct);
        }
        else
        {
            if (request.PasswordTemporal is null) return Errores.PasswordTemporalRequerida;

            acceso = new Acceso { Email = email, Verificado = true, DebeCambiarPassword = true };
            acceso.PasswordHash = hasher.Hashear(acceso, request.PasswordTemporal);
            repositorio.AgregarAcceso(acceso);
        }

        // Una cuenta sin persona (cuenta nueva, o una de staff sin vínculos) recibe una persona nueva.
        if (persona is null)
        {
            persona = new Persona
            {
                PrimerNombre = request.PrimerNombre.Trim(),
                SegundoNombre = Opcional(request.SegundoNombre),
                PrimerApellido = request.PrimerApellido.Trim(),
                SegundoApellido = Opcional(request.SegundoApellido)
            };
            repositorio.AgregarPersona(persona, new Usuario { PersonaId = persona.Id, Activo = true });
        }

        var vinculo = new VinculoEmpresa
        {
            PersonaId = persona.Id,
            AccesoId = acceso.Id,
            RolId = rol.Id,
            Estado = EstadoVinculo.Activo
        };
        repositorio.AgregarVinculo(vinculo);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return new CrearVinculoResponseDto(vinculo.Id, persona.Id, acceso.Email, persona.NombreCompleto, rol.Codigo,
            vinculo.Estado, vinculo.CreadoEn, cuentaNueva);
    }

    private static string? Opcional(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
