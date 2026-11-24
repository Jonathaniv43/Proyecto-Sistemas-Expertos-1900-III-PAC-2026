using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Plan;

/// <summary>Errores de negocio del módulo Plan de trabajo, compartidos solo por sus slices.</summary>
public static class Errores
{
    public static readonly Error SinVinculo = new("plan.sin_vinculo",
        "El plan de trabajo es de una empresa: inicie sesión con su vínculo.", TipoError.Prohibido);
}
