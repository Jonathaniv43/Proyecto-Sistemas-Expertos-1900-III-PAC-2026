using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Auth.CerrarSesion;

/// <summary>Cierra la sesión borrando su refresh (RN-66). Es idempotente: un token desconocido también da éxito.</summary>
public sealed class CerrarSesionServicio(
    ICerrarSesionRepositorio repo,
    IUnidadDeTrabajo uow,
    ITokenServicio tokens) : IServicioCasoDeUso
{
    public async Task<Resultado> EjecutarAsync(CerrarSesionRequest request, CancellationToken cancellationToken)
    {
        var fila = await repo.BuscarRefreshAsync(tokens.HashRefresh(request.RefreshToken), cancellationToken);
        if (fila is not null)
        {
            repo.EliminarRefresh(fila);
            await uow.GuardarCambiosAsync(cancellationToken);
        }

        return Resultado.Exito();
    }
}
