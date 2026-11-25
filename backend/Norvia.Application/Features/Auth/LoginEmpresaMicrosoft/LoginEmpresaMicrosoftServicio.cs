using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Common.Seguridad;
using Norvia.Application.Features.Auth.DTOs;
using Norvia.Application.Features.Auth.LoginEmpresaMicrosoft.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Auth.LoginEmpresaMicrosoft;

/// <summary>
/// Inicia sesión en una empresa con una cuenta de Microsoft ya vinculada (HU-120). Mismas restricciones que el
/// login con contraseña (RN-57, RN-60, RN-67) y mismo error genérico para no revelar qué falló.
/// </summary>
public sealed class LoginEmpresaMicrosoftServicio(
    ILoginEmpresaMicrosoftRepositorio repo,
    IUnidadDeTrabajo uow,
    ICifradorEmpresaId cifrador,
    ITokenIdentidadExterna tokensIdentidad,
    IReloj reloj,
    EmisorSesion emisor) : IServicioCasoDeUso
{
    public async Task<Resultado<SesionResponseDto>> EjecutarAsync(
        LoginEmpresaMicrosoftRequestDto request, CancellationToken cancellationToken)
    {
        var ahora = await reloj.AhoraAsync(cancellationToken);
        var identidad = tokensIdentidad.Leer(request.TokenIdentidad, ahora);
        if (identidad is null) return Errores.IdentidadInvalida;

        if (!cifrador.TryDescifrar(request.IdEmpresa, out var empresaId))
            return Errores.CredencialesInvalidas;

        var acceso = await repo.BuscarAccesoPorProveedorAsync(identidad.Proveedor, identidad.ProveedorUserId, cancellationToken);
        if (acceso is null || !acceso.Verificado) return Errores.CredencialesInvalidas;

        var vinculo = await repo.BuscarVinculoAsync(acceso.Id, empresaId, cancellationToken);
        if (vinculo is null
            || vinculo.Estado != EstadoVinculo.Activo
            || vinculo.Empresa.Estado != EstadoEmpresa.Activa
            || !await repo.UsuarioActivoAsync(vinculo.PersonaId, cancellationToken))
            return Errores.CredencialesInvalidas;

        var sesion = new SesionToken(acceso.Id, TipoSesion.Empresa, vinculo.Id, vinculo.EmpresaId, vinculo.Rol.Codigo, acceso.Email);
        // DebeCambiarPassword solo aplica a contraseñas temporales: sin contraseña no hay nada que cambiar.
        var (fila, respuesta) = emisor.Emitir(sesion, ahora, acceso.PasswordHash is not null && acceso.DebeCambiarPassword);

        repo.AgregarRefresh(fila);
        await uow.GuardarCambiosAsync(cancellationToken);
        return respuesta;
    }
}
