using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Common.Seguridad;
using Norvia.Application.Features.Auth.DTOs;
using Norvia.Application.Features.Auth.LoginEmpresa.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Auth.LoginEmpresa;

/// <summary>Inicia sesión en una empresa con el Acceso y el EmpresaId cifrado (RN-57, RN-60, RN-67).</summary>
public sealed class LoginEmpresaServicio(
    ILoginEmpresaRepositorio repo,
    IUnidadDeTrabajo uow,
    ICifradorEmpresaId cifrador,
    IHasherPassword hasher,
    IReloj reloj,
    EmisorSesion emisor) : IServicioCasoDeUso
{
    public async Task<Resultado<SesionResponseDto>> EjecutarAsync(LoginEmpresaRequestDto request, CancellationToken cancellationToken)
    {
        if (!cifrador.TryDescifrar(request.IdEmpresa, out var empresaId))
            return Errores.CredencialesInvalidas;

        var acceso = await repo.BuscarAccesoAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (acceso is null || !acceso.Verificado || !hasher.Verificar(acceso, request.Password))
            return Errores.CredencialesInvalidas;

        var vinculo = await repo.BuscarVinculoAsync(acceso.Id, empresaId, cancellationToken);
        if (vinculo is null
            || vinculo.Estado != EstadoVinculo.Activo
            || vinculo.Empresa.Estado != EstadoEmpresa.Activa
            || !await repo.UsuarioActivoAsync(vinculo.PersonaId, cancellationToken))
            return Errores.CredencialesInvalidas;

        var ahora = await reloj.AhoraAsync(cancellationToken);
        var sesion = new SesionToken(acceso.Id, TipoSesion.Empresa, vinculo.Id, vinculo.EmpresaId, vinculo.Rol.Codigo, acceso.Email, acceso.PasswordHash is not null);
        var (fila, respuesta) = emisor.Emitir(sesion, ahora);

        repo.AgregarRefresh(fila);
        await uow.GuardarCambiosAsync(cancellationToken);
        return respuesta;
    }
}
