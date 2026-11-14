using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Seguridad;
using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Auth;

/// <summary>
/// Arma el JWT de acceso y la fila de refresh de una sesión. No persiste nada:
/// cada slice agrega la fila con su repositorio y confirma con la unidad de trabajo.
/// Compartido solo por los slices del módulo Auth.
/// </summary>
public sealed class EmisorSesion(ITokenServicio tokens)
{
    public (RefreshToken Fila, SesionResponse Respuesta) Emitir(SesionToken sesion, DateTime ahoraUtc)
    {
        var (plano, hash) = tokens.CrearRefresh();

        var fila = new RefreshToken
        {
            AccesoId = sesion.AccesoId,
            VinculoEmpresaId = sesion.VinculoEmpresaId,
            TokenHash = hash,
            TipoSesion = sesion.Tipo,
            ExpiraEn = ahoraUtc.Add(tokens.RefreshDuracion)
        };

        var respuesta = new SesionResponse(tokens.CrearAccess(sesion, ahoraUtc), plano, tokens.AccessSegundos);
        return (fila, respuesta);
    }
}
