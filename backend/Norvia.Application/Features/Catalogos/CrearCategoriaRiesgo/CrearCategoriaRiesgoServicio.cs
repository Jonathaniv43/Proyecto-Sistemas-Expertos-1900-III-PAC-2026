using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Catalogos.DTOs;
using Norvia.Application.Features.Catalogos.CrearCategoriaRiesgo.DTOs;
using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.CrearCategoriaRiesgo;

/// <summary>Crea una categoría de riesgo dentro de un rubro activo.</summary>
public sealed class CrearCategoriaRiesgoServicio(
    ICrearCategoriaRiesgoRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo) : IServicioCasoDeUso
{
    public async Task<Resultado<CategoriaRiesgoResponseDto>> EjecutarAsync(
        CrearCategoriaRiesgoRequestDto request, CancellationToken ct)
    {
        if (!await repositorio.RubroActivoExisteAsync(request.RubroId, ct)) return Errores.RubroInvalido;

        var nombre = request.Nombre.Trim();
        if (await repositorio.ExisteNombreAsync(request.RubroId, nombre, ct)) return Errores.NombreDuplicado;

        var categoria = new CategoriaRiesgo
        {
            RubroId = request.RubroId,
            Nombre = nombre,
            Descripcion = Texto.Opcional(request.Descripcion),
            Activa = true
        };
        repositorio.Agregar(categoria);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return CategoriaRiesgoResponseDto.De(categoria);
    }
}
