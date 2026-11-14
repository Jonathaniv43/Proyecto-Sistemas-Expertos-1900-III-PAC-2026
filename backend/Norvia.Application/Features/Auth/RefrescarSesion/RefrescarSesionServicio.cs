using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Common.Seguridad;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Auth.RefrescarSesion;

/// <summary>
/// Rota el refresh token (RN-66): borra la fila usada y, si la sesión sigue siendo válida,
/// crea la nueva en la misma transacción.
/// </summary>
public sealed class RefrescarSesionServicio(
    IRefrescarSesionRepositorio repo,
    IUnidadDeTrabajo uow,
    ITokenServicio tokens,
    IReloj reloj,
    EmisorSesion emisor) : IServicioCasoDeUso
{
    public async Task<Resultado<SesionResponse>> EjecutarAsync(RefrescarSesionRequest request, CancellationToken cancellationToken)
    {
        var fila = await repo.BuscarRefreshAsync(tokens.HashRefresh(request.RefreshToken), cancellationToken);
        if (fila is null)
            return Errores.SesionInvalida;

        // Un refresh se usa una sola vez, sea válido o no.
        repo.EliminarRefresh(fila);

        var ahora = await reloj.AhoraAsync(cancellationToken);
        var sesion = fila.ExpiraEn > ahora && fila.Acceso.Verificado ? ResolverSesion(fila) : null;
        if (sesion is null)
        {
            await uow.GuardarCambiosAsync(cancellationToken);
            return Errores.SesionInvalida;
        }

        var (nueva, respuesta) = emisor.Emitir(sesion, ahora);
        repo.AgregarRefresh(nueva);
        await uow.GuardarCambiosAsync(cancellationToken);   // borrar + crear: una sola transacción
        return respuesta;
    }

    private static SesionToken? ResolverSesion(RefreshToken fila)
    {
        if (fila.TipoSesion == TipoSesion.Empresa)
        {
            var v = fila.VinculoEmpresa;
            return v is not null && v.Estado == EstadoVinculo.Activo && v.Empresa.Estado == EstadoEmpresa.Activa
                ? new SesionToken(fila.AccesoId, TipoSesion.Empresa, v.Id, v.EmpresaId, v.Rol.Codigo)
                : null;
        }

        var staff = fila.Acceso.Staff;
        return staff is not null && staff.Activo
            ? new SesionToken(fila.AccesoId, TipoSesion.Plataforma, null, null, staff.RolStaffPlataforma.Codigo)
            : null;
    }
}
