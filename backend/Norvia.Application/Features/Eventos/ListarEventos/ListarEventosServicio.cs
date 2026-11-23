using System.Text.Json;
using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Eventos.ListarEventos.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Eventos.ListarEventos;

/// <summary>HU-047: consulta resumida de eventos de la empresa con filtros opcionales en query.</summary>
public sealed class ListarEventosServicio(IListarEventosRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<IReadOnlyList<EventoResumenResponseDto>>> EjecutarAsync(
        string? estado, string? tipo, bool? esGrave, CancellationToken ct)
    {
        if (!IntentarLeerEnum(estado, out EstadoAccidente? estadoEnum))
            return Errores.FiltroInvalido("estado", estado!);

        if (!IntentarLeerEnum(tipo, out TipoAccidente? tipoEnum))
            return Errores.FiltroInvalido("tipo", tipo!);

        var lista = await repositorio.ListarAsync(estadoEnum, tipoEnum, esGrave, ct);
        return Resultado<IReadOnlyList<EventoResumenResponseDto>>.Exito(lista);
    }

    private static bool IntentarLeerEnum<TEnum>(string? raw, out TEnum? valor)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            valor = null;
            return true;
        }

        var limpio = raw.Trim();
        foreach (var candidato in Enum.GetValues<TEnum>())
        {
            var enSnake = JsonNamingPolicy.SnakeCaseLower.ConvertName(candidato.ToString());
            if (string.Equals(enSnake, limpio, StringComparison.Ordinal))
            {
                valor = candidato;
                return true;
            }
        }

        valor = null;
        return false;
    }
}
