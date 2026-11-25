using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Auth.VerificarCodigoMicrosoft.DTOs;

namespace Norvia.Application.Features.Auth.VerificarCodigoMicrosoft;

/// <summary>
/// Comprueba el código de 6 dígitos contra la fila de <see cref="Norvia.Domain.Entities.Acceso.VerificacionCorreoMicrosoft"/>
/// y devuelve el token de identidad con el correo confirmado (RN-86). 6 dígitos son un millón de combinaciones, así
/// que los intentos fallidos están limitados. Una vez confirmado, la fila se borra: el código sirve una sola vez.
/// </summary>
public sealed class VerificarCodigoMicrosoftServicio(
    IVerificarCodigoMicrosoftRepositorio repo,
    IUnidadDeTrabajo uow,
    ITokenIdentidadExterna tokensIdentidad,
    IHashCodigoVerificacion hashCodigo,
    IReloj reloj) : IServicioCasoDeUso
{
    private const int MinutosVigenciaToken = 10;
    private const int MaximoIntentos = 5;

    public async Task<Resultado<VerificarCodigoMicrosoftResponseDto>> EjecutarAsync(
        VerificarCodigoMicrosoftRequestDto request, CancellationToken cancellationToken)
    {
        var ahora = await reloj.AhoraAsync(cancellationToken);
        var identidad = tokensIdentidad.Leer(request.TokenIdentidad, ahora);
        if (identidad is null) return Errores.IdentidadInvalida;

        var fila = await repo.BuscarAsync(identidad.Email, identidad.ProveedorUserId, cancellationToken);
        if (fila is null || fila.ExpiraEn <= ahora) return Errores.CodigoIncorrecto;
        if (fila.Intentos >= MaximoIntentos) return Errores.DemasiadosIntentos;

        if (!hashCodigo.Coincide(fila.CodigoHash, request.Codigo))
        {
            fila.Intentos++;
            await uow.GuardarCambiosAsync(cancellationToken);
            return Errores.CodigoIncorrecto;
        }

        repo.Eliminar(fila);
        await uow.GuardarCambiosAsync(cancellationToken);

        var confirmada = identidad with { CorreoVerificado = true };
        return new VerificarCodigoMicrosoftResponseDto(tokensIdentidad.Crear(confirmada, ahora.AddMinutes(MinutosVigenciaToken)));
    }
}
