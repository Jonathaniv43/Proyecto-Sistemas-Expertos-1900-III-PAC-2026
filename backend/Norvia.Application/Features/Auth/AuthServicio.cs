using Norvia.Application.Features.Auth.DTOs;
using Norvia.Application.Features.Auth.Interfaces;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Auth;

/// <summary>Casos de uso de login/refresh/logout. Reglas de negocio aquí; crypto y EF en Infra.</summary>
public class AuthServicio(
    IAuthRepositorio repo,
    ITokenServicio tokens,
    IRelojBaseDatos reloj,
    ICifradorEmpresaId cifrador,
    IVerificadorPassword passwords) : IAuthServicio
{
    public async Task<ResultadoAuth> LoginEmpresaAsync(LoginEmpresaRequest request, CancellationToken cancellationToken = default)
    {
        if (!cifrador.TryDescifrar(request.IdEmpresa, out var empresaId))
            return ResultadoAuth.Fallo();

        var acceso = await repo.BuscarAccesoPorEmailAsync(NormalizarEmail(request.Email), cancellationToken);
        if (!AccesoPuedeAutenticarse(acceso, request.Password))
            return ResultadoAuth.Fallo();

        var vinculo = await repo.BuscarVinculoConEmpresaYRolAsync(acceso!.Id, empresaId, cancellationToken);
        if (!VinculoEmpresaValido(vinculo))
            return ResultadoAuth.Fallo();

        return await EmitirAsync(
            new SesionToken(acceso.Id, TipoSesion.Empresa, vinculo!.Id, vinculo.EmpresaId, vinculo.Rol.Codigo),
            acceso.Id,
            vinculo.Id,
            cancellationToken);
    }

    public async Task<ResultadoAuth> LoginPlataformaAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var acceso = await repo.BuscarAccesoPorEmailAsync(NormalizarEmail(request.Email), cancellationToken);
        if (!AccesoPuedeAutenticarse(acceso, request.Password))
            return ResultadoAuth.Fallo();

        var staff = await repo.BuscarStaffConRolAsync(acceso!.Id, cancellationToken);
        if (!StaffValido(staff))
            return ResultadoAuth.Fallo();

        return await EmitirAsync(
            new SesionToken(acceso.Id, TipoSesion.Plataforma, null, null, staff!.RolStaffPlataforma.Codigo),
            acceso.Id,
            null,
            cancellationToken);
    }

    public async Task<ResultadoAuth> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var hash = tokens.HashRefresh(request.RefreshToken);
        var ahora = await reloj.AhoraAsync(cancellationToken);
        var fila = await repo.BuscarRefreshConSesionAsync(hash, cancellationToken);

        if (fila is null)
            return ResultadoAuth.Fallo();

        await repo.EliminarRefreshAsync(fila, cancellationToken);

        if (fila.ExpiraEn <= ahora || !fila.Acceso.Verificado)
            return ResultadoAuth.Fallo();

        SesionToken sesion;
        if (fila.TipoSesion == TipoSesion.Empresa)
        {
            if (!VinculoEmpresaValido(fila.VinculoEmpresa))
                return ResultadoAuth.Fallo();
            var v = fila.VinculoEmpresa!;
            sesion = new SesionToken(fila.AccesoId, TipoSesion.Empresa, v.Id, v.EmpresaId, v.Rol.Codigo);
        }
        else
        {
            if (!StaffValido(fila.Acceso.Staff))
                return ResultadoAuth.Fallo();
            var staff = fila.Acceso.Staff!;
            sesion = new SesionToken(fila.AccesoId, TipoSesion.Plataforma, null, null, staff.RolStaffPlataforma.Codigo);
        }

        return await EmitirAsync(sesion, fila.AccesoId, fila.VinculoEmpresaId, cancellationToken);
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        var hash = tokens.HashRefresh(request.RefreshToken);
        var fila = await repo.BuscarRefreshConSesionAsync(hash, cancellationToken);
        if (fila is null)
            return;
        await repo.EliminarRefreshAsync(fila, cancellationToken);
    }

    private bool AccesoPuedeAutenticarse(Acceso? acceso, string password) =>
        acceso is not null
        && acceso.Verificado
        && passwords.Verificar(acceso, password);

    private static bool VinculoEmpresaValido(VinculoEmpresa? vinculo) =>
        vinculo is not null
        && vinculo.Estado == EstadoVinculo.Activo
        && vinculo.Empresa.Estado == EstadoEmpresa.Activa;

    private static bool StaffValido(StaffPlataforma? staff) =>
        staff is not null && staff.Activo;

    private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

    private async Task<ResultadoAuth> EmitirAsync(
        SesionToken sesion,
        Guid accesoId,
        Guid? vinculoId,
        CancellationToken cancellationToken)
    {
        var ahora = await reloj.AhoraAsync(cancellationToken);
        var access = tokens.CrearAccess(sesion, ahora);
        var (plano, hash) = tokens.CrearRefresh();

        await repo.AgregarRefreshAsync(new RefreshToken
        {
            AccesoId = accesoId,
            VinculoEmpresaId = vinculoId,
            TokenHash = hash,
            TipoSesion = sesion.Tipo,
            ExpiraEn = ahora.Add(tokens.RefreshDuracion)
        }, cancellationToken);

        return ResultadoAuth.Exito(new TokenResponse
        {
            AccessToken = access,
            RefreshToken = plano,
            ExpiresIn = tokens.AccessSegundos
        });
    }
}
