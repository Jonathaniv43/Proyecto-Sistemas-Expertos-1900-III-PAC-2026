using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Vinculos.CrearEncuestado.DTOs;
using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Vinculos.CrearEncuestado;

/// <summary>
/// Alta individual de un encuestado (HU-115, RN-74): rol fijo <c>encuestado</c>, que solo responde encuestas,
/// ve sus notificaciones y consulta las políticas vigentes.
/// </summary>
public sealed class CrearEncuestadoServicio(AltaDeVinculo alta, IUnidadDeTrabajo unidadDeTrabajo) : IServicioCasoDeUso
{
    public const string RolCodigo = "encuestado";

    public async Task<Resultado<CrearEncuestadoResponseDto>> EjecutarAsync(CrearEncuestadoRequestDto request, CancellationToken ct)
    {
        var datos = new DatosPersonaAlta(request.Email, request.PrimerNombre, request.SegundoNombre,
            request.PrimerApellido, request.SegundoApellido);
        var preparado = await alta.PrepararAsync(datos, Rol.EncuestadoId, request.PasswordTemporal, generarSiFalta: true, ct);
        if (!preparado.EsExito) return preparado.Error!;

        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        var (vinculo, persona, acceso, cuentaNueva, generada) = preparado.Valor;
        return new CrearEncuestadoResponseDto(vinculo.Id, persona.Id, acceso.Email, persona.NombreCompleto, RolCodigo,
            vinculo.Estado, vinculo.CreadoEn, cuentaNueva, generada);
    }
}
