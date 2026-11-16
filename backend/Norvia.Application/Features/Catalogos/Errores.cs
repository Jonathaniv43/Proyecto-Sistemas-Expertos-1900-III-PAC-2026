using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Catalogos;

/// <summary>Errores de negocio del módulo Catalogos (HU-023), compartidos solo por sus slices.</summary>
public static class Errores
{
    public static readonly Error RubroNoEncontrado = new("catalogos.rubro_no_encontrado", "El rubro no existe.", TipoError.NoEncontrado);
    public static readonly Error CategoriaNoEncontrada = new("catalogos.categoria_no_encontrada", "La categoría de riesgo no existe.", TipoError.NoEncontrado);
    public static readonly Error PeligroNoEncontrado = new("catalogos.peligro_no_encontrado", "El peligro no existe.", TipoError.NoEncontrado);
    public static readonly Error PreguntaNoEncontrada = new("catalogos.pregunta_no_encontrada", "La pregunta no existe.", TipoError.NoEncontrado);
    public static readonly Error PlantillaNoEncontrada = new("catalogos.plantilla_no_encontrada", "La plantilla no existe.", TipoError.NoEncontrado);

    // Referencias a otros elementos del catálogo que no existen o están desactivados.
    public static readonly Error RubroInvalido = new("catalogos.rubro_invalido", "El rubro indicado no existe o está desactivado.", TipoError.Validacion);
    public static readonly Error CategoriaInvalida = new("catalogos.categoria_invalida", "La categoría indicada no existe o está desactivada.", TipoError.Validacion);
    public static readonly Error PeligroInvalido = new("catalogos.peligro_invalido", "El peligro indicado no existe o está desactivado.", TipoError.Validacion);

    public static readonly Error CodigoDuplicado = new("catalogos.codigo_duplicado", "Ya existe un elemento con ese código.", TipoError.Conflicto);
    public static readonly Error NombreDuplicado = new("catalogos.nombre_duplicado", "Ya existe un elemento con ese nombre.", TipoError.Conflicto);
    public static readonly Error OrdenOcupado = new("catalogos.orden_ocupado", "Otra pregunta de la categoría ya tiene esa posición.", TipoError.Conflicto);
}
