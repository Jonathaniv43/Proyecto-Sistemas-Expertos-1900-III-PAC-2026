using Norvia.Application.Common.Auditoria;
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
public sealed class CrearEncuestadoServicio(AltaDeVinculo alta, IUnidadDeTrabajo unidadDeTrabajo, Auditor auditor) : IServicioCasoDeUso
{
    public const string RolCodigo = "encuestado";

    public async Task<Resultado<CrearEncuestadoResponseDto>> EjecutarAsync(CrearEncuestadoRequestDto request, CancellationToken ct)
    {
        var datos = new DatosPersonaAlta(request.Email, request.PrimerNombre, request.SegundoNombre,
            request.PrimerApellido, request.SegundoApellido);
        var preparado = await alta.PrepararAsync(datos, Rol.EncuestadoId, "Encuestado", ct);
        if (!preparado.EsExito) return preparado.Error!;

        var (vinculo, persona, acceso, _, _) = preparado.Valor;
        var cuentaNueva = preparado.Valor.CuentaNueva;
        auditor.Registrar(AccionesAuditoria.EncuestadoCreado, EntidadesAuditoria.VinculoEmpresa, vinculo.Id,
            new { persona = persona.NombreCompleto, email = acceso.Email, rol = RolCodigo, cuentaNueva });
        await unidadDeTrabajo.GuardarCambiosAsync(ct);
        var correoActivacionEnviado = await alta.EnviarActivacionAsync(preparado.Valor, ct);

        return new CrearEncuestadoResponseDto(vinculo.Id, persona.Id, acceso.Email, persona.NombreCompleto, RolCodigo,
            vinculo.Estado, vinculo.CreadoEn, cuentaNueva, correoActivacionEnviado);
    }
}
