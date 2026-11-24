using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Eventos.DTOs;

namespace Norvia.Application.Features.Eventos.ListarMedidasCorrectivas;

/// <summary>
/// HU-050: las medidas correctivas de un evento con su avance (estado, responsable y fecha límite de cada tarea).
/// Orden: las abiertas primero (pendientes y en progreso), por fecha límite; luego completadas y canceladas.
/// </summary>
public sealed class ListarMedidasCorrectivasServicio(IListarMedidasCorrectivasRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<IReadOnlyList<MedidaCorrectivaResponseDto>>> EjecutarAsync(Guid eventoId, CancellationToken ct)
    {
        if (!await repositorio.ExisteAsync(eventoId, ct)) return Errores.EventoNoEncontrado;
        return await repositorio.ListarAsync(eventoId, ct);
    }
}
