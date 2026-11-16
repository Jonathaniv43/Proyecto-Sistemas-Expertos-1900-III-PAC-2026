using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Catalogos.DTOs;
using Norvia.Application.Features.Catalogos.CrearRubro.DTOs;
using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.CrearRubro;

public sealed class CrearRubroServicio(ICrearRubroRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo)
    : IServicioCasoDeUso
{
    public async Task<Resultado<RubroResponseDto>> EjecutarAsync(CrearRubroRequestDto request, CancellationToken ct)
    {
        var codigo = request.Codigo.Trim();
        var nombre = request.Nombre.Trim();
        if (await repositorio.ExisteCodigoAsync(codigo, ct)) return Errores.CodigoDuplicado;
        if (await repositorio.ExisteNombreAsync(nombre, ct)) return Errores.NombreDuplicado;

        var rubro = new Rubro
        {
            Id = (short)(await repositorio.MayorIdAsync(ct) + 1),
            Codigo = codigo,
            Nombre = nombre,
            Activo = true
        };
        repositorio.Agregar(rubro);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return RubroResponseDto.De(rubro);
    }
}
