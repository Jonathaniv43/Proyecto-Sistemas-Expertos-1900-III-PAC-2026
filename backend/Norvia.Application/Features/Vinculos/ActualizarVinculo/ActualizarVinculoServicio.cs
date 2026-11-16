using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Vinculos.ActualizarVinculo.DTOs;
using Norvia.Application.Features.Vinculos.DTOs;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Vinculos.ActualizarVinculo;

/// <summary>
/// Cambia el rol o el estado del acceso de una persona a la empresa de la sesión (HU-021).
/// <list type="bullet">
/// <item>Cada cambio de estado se registra en el historial, en la misma transacción (RN-59).</item>
/// <item>La empresa conserva al menos un administrador activo (RN-08).</item>
/// <item>Suspender o dar de baja solo afecta a esta empresa: los vínculos con otras siguen igual.</item>
/// </list>
/// </summary>
public sealed class ActualizarVinculoServicio(
    IActualizarVinculoRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado<VinculoResponseDto>> EjecutarAsync(
        Guid id, ActualizarVinculoRequestDto request, CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid registradoPor) return Errores.VinculoNoEncontrado;

        var vinculo = await repositorio.BuscarAsync(id, ct);
        if (vinculo is null) return Errores.VinculoNoEncontrado;

        var rolNuevo = vinculo.Rol;
        if (request.RolCodigo is not null)
        {
            rolNuevo = await repositorio.BuscarRolAsync(request.RolCodigo.Trim(), ct);
            if (rolNuevo is null) return Errores.RolInvalido;
        }
        var estadoNuevo = request.Estado ?? vinculo.Estado;

        // RN-08: si hoy es administrador activo y deja de serlo, debe quedar otro.
        var esAdminActivo = vinculo.RolId == Rol.AdminEmpresaId && vinculo.Estado == EstadoVinculo.Activo;
        var seguiraAdminActivo = rolNuevo.Id == Rol.AdminEmpresaId && estadoNuevo == EstadoVinculo.Activo;
        if (esAdminActivo && !seguiraAdminActivo
            && await repositorio.ContarOtrosAdministradoresActivosAsync(vinculo.Id, ct) == 0)
            return Errores.UltimoAdministrador;

        if (estadoNuevo != vinculo.Estado)
        {
            repositorio.AgregarHistorial(new HistorialEstadoVinculo
            {
                VinculoEmpresaId = vinculo.Id,
                EstadoAnterior = vinculo.Estado,
                EstadoNuevo = estadoNuevo,
                MotivoCodigo = request.MotivoCodigo ?? MotivoEstadoVinculo.DecisionAdmin, // solo al reactivar
                MotivoDetalle = string.IsNullOrWhiteSpace(request.MotivoDetalle) ? null : request.MotivoDetalle.Trim(),
                RegistradoPorId = registradoPor
            });
            if (estadoNuevo != EstadoVinculo.Activo)
                await repositorio.EliminarRefreshDelVinculoAsync(vinculo.Id, ct);
            vinculo.Estado = estadoNuevo;
        }

        vinculo.RolId = rolNuevo.Id;
        vinculo.Rol = rolNuevo;
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return VinculoResponseDto.De(vinculo);
    }
}
