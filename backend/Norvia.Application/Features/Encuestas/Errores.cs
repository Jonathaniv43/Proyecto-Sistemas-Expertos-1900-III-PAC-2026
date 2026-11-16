using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Encuestas;

/// <summary>Errores de negocio del módulo Encuestas, compartidos solo por sus slices.</summary>
public static class Errores
{
    /// <summary>RN-10: sin rubro no hay categorías que elegir ni encuesta que generar.</summary>
    public static readonly Error EmpresaSinRubro = new("encuestas.empresa_sin_rubro",
        "La empresa todavía no tiene rubro. Complete el perfil de la empresa para elegir categorías de riesgo.",
        TipoError.Conflicto);
}
