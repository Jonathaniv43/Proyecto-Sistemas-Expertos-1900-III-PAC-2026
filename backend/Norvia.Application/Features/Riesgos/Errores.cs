using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Riesgos;

/// <summary>Errores de negocio del módulo Riesgos, compartidos solo por sus slices.</summary>
public static class Errores
{
    public static readonly Error RiesgoNoEncontrado = new("riesgos.no_encontrado",
        "El riesgo no existe en esta empresa.", TipoError.NoEncontrado);

    public static readonly Error PeligroInvalido = new("riesgos.peligro_invalido",
        "El peligro indicado no existe o ya no está disponible en el catálogo.", TipoError.Validacion);

    public static readonly Error RiesgoDuplicado = new("riesgos.duplicado",
        "Ese riesgo ya está en la matriz con la misma actividad.", TipoError.Conflicto);
}
