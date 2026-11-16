using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Catalogos.DTOs;
using Norvia.Application.Features.Catalogos.ActualizarCategoriaRiesgo.DTOs;

namespace Norvia.Application.Features.Catalogos.ActualizarCategoriaRiesgo;

/// <summary>Edita o mueve de rubro una categoría de riesgo.</summary>
public sealed class ActualizarCategoriaRiesgoServicio(
    IActualizarCategoriaRiesgoRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo) : IServicioCasoDeUso
{
    public async Task<Resultado<CategoriaRiesgoResponseDto>> EjecutarAsync(
        Guid id, ActualizarCategoriaRiesgoRequestDto request, CancellationToken ct)
    {
        var categoria = await repositorio.BuscarAsync(id, ct);
        if (categoria is null) return Errores.CategoriaNoEncontrada;

        var rubroId = request.RubroId ?? categoria.RubroId;
        if (rubroId != categoria.RubroId && !await repositorio.RubroActivoExisteAsync(rubroId, ct))
            return Errores.RubroInvalido;

        var nombre = request.Nombre?.Trim() ?? categoria.Nombre;
        if ((rubroId != categoria.RubroId || nombre != categoria.Nombre)
            && await repositorio.ExisteNombreEnOtraAsync(id, rubroId, nombre, ct))
            return Errores.NombreDuplicado;

        categoria.RubroId = rubroId;
        categoria.Nombre = nombre;
        if (request.Descripcion is not null) categoria.Descripcion = Texto.Opcional(request.Descripcion);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return CategoriaRiesgoResponseDto.De(categoria);
    }
}
