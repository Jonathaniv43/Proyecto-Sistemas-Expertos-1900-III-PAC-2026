using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Common.Seguridad;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Auth.LoginPlataforma;

/// <summary>Inicia sesión del staff de Norvia (StaffPlataforma, D23, RN-64).</summary>
public sealed class LoginPlataformaServicio(
    ILoginPlataformaRepositorio repo,
    IUnidadDeTrabajo uow,
    IHasherPassword hasher,
    IReloj reloj,
    EmisorSesion emisor) : IServicioCasoDeUso
{
    public async Task<Resultado<SesionResponse>> EjecutarAsync(LoginPlataformaRequest request, CancellationToken cancellationToken)
    {
        var acceso = await repo.BuscarAccesoAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (acceso is null || !acceso.Verificado || !hasher.Verificar(acceso, request.Password))
            return Errores.CredencialesInvalidas;

        var staff = await repo.BuscarStaffAsync(acceso.Id, cancellationToken);
        if (staff is null || !staff.Activo)
            return Errores.CredencialesInvalidas;

        var ahora = await reloj.AhoraAsync(cancellationToken);
        var sesion = new SesionToken(acceso.Id, TipoSesion.Plataforma, null, null, staff.RolStaffPlataforma.Codigo);
        var (fila, respuesta) = emisor.Emitir(sesion, ahora);

        repo.AgregarRefresh(fila);
        await uow.GuardarCambiosAsync(cancellationToken);
        return respuesta;
    }
}
