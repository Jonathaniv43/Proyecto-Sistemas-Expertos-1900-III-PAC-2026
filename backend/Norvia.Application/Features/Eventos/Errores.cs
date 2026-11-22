using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Eventos;

/// <summary>Errores de negocio del módulo Eventos / M6, compartidos solo por sus slices.</summary>
public static class Errores
{
    public static readonly Error EventoNoEncontrado = new("eventos.no_encontrado",
        "El evento no existe.",
        TipoError.NoEncontrado);

    public static readonly Error FechaEventoFutura = new("eventos.fecha_futura",
        "La fecha y hora del evento no puede ser futura.",
        TipoError.Validacion);

    public static Error FiltroInvalido(string campo, string valor) => new("eventos.filtro_invalido",
        $"El filtro '{campo}' no acepta el valor '{valor}'.",
        TipoError.Validacion);
}
