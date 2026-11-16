using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Catalogos.DTOs;
using Norvia.Application.Features.Catalogos.CrearPeligro.DTOs;
using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.CrearPeligro;

public sealed class CrearPeligroServicio(ICrearPeligroRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo)
    : IServicioCasoDeUso
{
    public async Task<Resultado<PeligroResponseDto>> EjecutarAsync(CrearPeligroRequestDto request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim();
        if (await repositorio.ExisteCodigoAsync(codigo, ct)) return Errores.CodigoDuplicado;

        var peligro = new Peligro
        {
            Codigo = codigo,
            Nombre = request.Nombre.Trim(),
            Descripcion = Texto.Opcional(request.Descripcion),
            Clasificacion = request.Clasificacion,
            Activo = true
        };
        repositorio.Agregar(peligro);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return PeligroResponseDto.De(peligro);
    }
}
