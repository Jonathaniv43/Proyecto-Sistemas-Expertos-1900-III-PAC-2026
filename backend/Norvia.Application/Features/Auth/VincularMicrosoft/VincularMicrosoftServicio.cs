using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Auth.VincularMicrosoft.DTOs;
using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Auth.VincularMicrosoft;

/// <summary>
/// Vincula una cuenta de Microsoft a un Acceso con contraseña, confirmando esa contraseña (RN-83).
/// Después la persona puede entrar con cualquiera de las dos formas (HU-120).
/// </summary>
public sealed class VincularMicrosoftServicio(
    IVincularMicrosoftRepositorio repo,
    IUnidadDeTrabajo uow,
    ITokenIdentidadExterna tokensIdentidad,
    IHasherPassword hasher,
    IReloj reloj) : IServicioCasoDeUso
{
    public async Task<Resultado> EjecutarAsync(VincularMicrosoftRequestDto request, CancellationToken cancellationToken)
    {
        var ahora = await reloj.AhoraAsync(cancellationToken);
        var identidad = tokensIdentidad.Leer(request.TokenIdentidad, ahora);
        if (identidad is null) return Errores.IdentidadInvalida;

        if (await repo.ProveedorVinculadoAsync(identidad.Proveedor, identidad.ProveedorUserId, cancellationToken))
            return Errores.ProveedorYaVinculado;

        // El correo sale del token firmado, nunca del cliente (RN-84). Mismo error genérico si algo no coincide.
        var acceso = await repo.BuscarAccesoPorEmailAsync(identidad.Email, cancellationToken);
        if (acceso is null || !acceso.Verificado || !hasher.Verificar(acceso, request.Password))
            return Errores.CredencialesInvalidas;

        repo.Agregar(new AccesoProveedor
        {
            AccesoId = acceso.Id,
            Proveedor = identidad.Proveedor,
            ProveedorUserId = identidad.ProveedorUserId
        });
        await uow.GuardarCambiosAsync(cancellationToken);
        return Resultado.Exito();
    }
}
