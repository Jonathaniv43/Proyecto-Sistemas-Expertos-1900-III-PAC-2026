using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Catalogos.DTOs;
using Norvia.Application.Features.Catalogos.ActualizarPeligro.DTOs;

namespace Norvia.Application.Features.Catalogos.ActualizarPeligro;

/// <summary>Edita un peligro. Los riesgos de las empresas y las encuestas ya generadas lo siguen referenciando.</summary>
public sealed class ActualizarPeligroServicio(IActualizarPeligroRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo)
    : IServicioCasoDeUso
{
    public async Task<Resultado<PeligroResponseDto>> EjecutarAsync(
        Guid id, ActualizarPeligroRequestDto request, CancellationToken ct)
    {
        var peligro = await repositorio.BuscarAsync(id, ct);
        if (peligro is null) return Errores.PeligroNoEncontrado;

        if (request.Nombre is not null) peligro.Nombre = request.Nombre.Trim();
        if (request.Descripcion is not null) peligro.Descripcion = Texto.Opcional(request.Descripcion);
        if (request.Clasificacion is { } clasificacion) peligro.Clasificacion = clasificacion;
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return PeligroResponseDto.De(peligro);
    }
}
