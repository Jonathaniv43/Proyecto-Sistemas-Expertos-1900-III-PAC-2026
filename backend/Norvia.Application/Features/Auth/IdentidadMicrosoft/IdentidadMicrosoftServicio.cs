using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Auth.IdentidadMicrosoft.DTOs;

namespace Norvia.Application.Features.Auth.IdentidadMicrosoft;

/// <summary>
/// Canjea el código de Microsoft, valida la identidad y dice en qué estado está esa cuenta (D29, HU-119, HU-120).
/// Solo devuelve los nombres de una persona ya guardada si la cuenta de Microsoft ya está vinculada.
/// </summary>
public sealed class IdentidadMicrosoftServicio(
    IProveedorIdentidad proveedor,
    ITokenIdentidadExterna tokens,
    IIdentidadMicrosoftRepositorio repo,
    IReloj reloj) : IServicioCasoDeUso
{
    public const int MinutosVigencia = 10;

    public async Task<Resultado<IdentidadMicrosoftResponseDto>> EjecutarAsync(
        IdentidadMicrosoftRequestDto request, CancellationToken cancellationToken)
    {
        var canje = await proveedor.CanjearCodigoAsync(request.Codigo, request.CodeVerifier, request.RedirectUri, cancellationToken);
        if (!canje.EsExito) return canje.Error!;
        var identidad = canje.Valor;

        var ahora = await reloj.AhoraAsync(cancellationToken);
        var token = tokens.Crear(identidad, ahora.AddMinutes(MinutosVigencia));

        var vinculado = await repo.BuscarAccesoPorProveedorAsync(identidad.Proveedor, identidad.ProveedorUserId, cancellationToken);
        if (vinculado is not null)
        {
            var persona = await repo.BuscarPersonaDeAccesoAsync(vinculado.Id, cancellationToken);
            return persona is null
                ? Respuesta(token, identidad, EstadosIdentidad.CuentaVinculada, false)
                : new IdentidadMicrosoftResponseDto(
                    token, vinculado.Email, persona.PrimerNombre, persona.SegundoNombre,
                    persona.PrimerApellido, persona.SegundoApellido, EstadosIdentidad.CuentaVinculada, true);
        }

        var existente = await repo.BuscarAccesoPorEmailAsync(identidad.Email, cancellationToken);
        if (existente is null) return Respuesta(token, identidad, EstadosIdentidad.SinCuenta, false);

        // Con contraseña: se puede vincular confirmándola. Sin contraseña es la cuenta de otro Microsoft: no hay cómo confirmar.
        return existente.PasswordHash is null
            ? Errores.ProveedorYaVinculado
            : Respuesta(token, identidad, EstadosIdentidad.CuentaConContrasena, false);
    }

    private static IdentidadMicrosoftResponseDto Respuesta(string token, IdentidadExterna i, string estado, bool personaExistente) =>
        new(token, i.Email, i.PrimerNombre, i.SegundoNombre, i.PrimerApellido, i.SegundoApellido, estado, personaExistente);
}
