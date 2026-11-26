using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.AccesoRecurso.PermitirAccesoRecurso;

/// <summary>
/// Evalúa si la petición autenticada puede ejecutar el endpoint (catálogo Permiso + rol).
/// Devuelve null si debe continuar; Error si hay que rechazar.
/// </summary>
public sealed class PermitirAccesoRecursoServicio(IPermitirAccesoRecursoRepositorio repo) : IServicioCasoDeUso
{
    public async Task<Error?> EvaluarAsync(ContextoAccesoRecurso ctx, ITenantActual tenant, CancellationToken ct)
    {
        if (ctx.OmitirVerificacion)
            return null;

        if (!ctx.EstaAutenticado)
            return Errores.NoAutenticado;

        var permiso = await repo.BuscarPermisoAsync(ctx.MetodoHttp, ctx.RutaPatron, ct);
        if (permiso is null)
            return Errores.PermisoNoDisponible;

        var permitido = (tenant.TipoSesion, permiso.Alcance) switch
        {
            (TipoSesion.Empresa, AlcancePermiso.Empresa) when tenant.VinculoEmpresaId is Guid vinculo =>
                await repo.VinculoTienePermisoAsync(vinculo, permiso.Id, ct),
            (TipoSesion.Plataforma, AlcancePermiso.Plataforma) when ctx.AccesoId is Guid acceso =>
                await repo.StaffTienePermisoAsync(acceso, permiso.Id, ct),
            _ => false
        };

        return permitido ? null : Errores.PermisoProhibido;
    }
}
