using Norvia.Application.Common.Auditoria;
using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Vinculos.CrearVinculo.DTOs;

namespace Norvia.Application.Features.Vinculos.CrearVinculo;

/// <summary>
/// Da acceso a la empresa de la sesión con el rol elegido (HU-020, RN-02, D5). Una cuenta nueva no recibe
/// contraseña del administrador: le llega un código de 6 dígitos para activarla (RN-86).
/// </summary>
public sealed class CrearVinculoServicio(
    ICrearVinculoRepositorio repositorio,
    AltaDeVinculo alta,
    IUnidadDeTrabajo unidadDeTrabajo,
    Auditor auditor) : IServicioCasoDeUso
{
    public async Task<Resultado<CrearVinculoResponseDto>> EjecutarAsync(CrearVinculoRequestDto request, CancellationToken ct)
    {
        var rol = await repositorio.BuscarRolAsync(request.RolCodigo.Trim(), ct);
        if (rol is null) return Errores.RolInvalido;

        var datos = new DatosPersonaAlta(request.Email, request.PrimerNombre, request.SegundoNombre,
            request.PrimerApellido, request.SegundoApellido);
        var preparado = await alta.PrepararAsync(datos, rol.Id, rol.Nombre, ct);
        if (!preparado.EsExito) return preparado.Error!;

        var (vinculo, persona, acceso, cuentaNueva, correoActivacionEnviado) = preparado.Valor;
        auditor.Registrar(AccionesAuditoria.VinculoCreado, EntidadesAuditoria.VinculoEmpresa, vinculo.Id,
            new { persona = persona.NombreCompleto, email = acceso.Email, rol = rol.Codigo, cuentaNueva });
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return new CrearVinculoResponseDto(vinculo.Id, persona.Id, acceso.Email, persona.NombreCompleto, rol.Codigo,
            vinculo.Estado, vinculo.CreadoEn, cuentaNueva, correoActivacionEnviado);
    }
}
