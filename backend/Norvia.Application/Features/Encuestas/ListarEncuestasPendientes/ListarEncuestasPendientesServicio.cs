using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Features.Encuestas.ListarEncuestasPendientes.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.ListarEncuestasPendientes;

/// <summary>
/// HU-031: encuestas publicadas de la empresa que el vínculo de la sesión aún no ha respondido.
/// </summary>
public sealed class ListarEncuestasPendientesServicio(
    IListarEncuestasPendientesRepositorio repositorio,
    ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<IReadOnlyList<EncuestaPendienteResponseDto>> EjecutarAsync(CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid vinculoId)
            return [];

        var encuestas = await repositorio.ListarPublicadasSinParticipacionAsync(vinculoId, ct);

        return encuestas
            .Select(e =>
            {
                var version = e.Versiones.First(v => v.Estado == EstadoVersionEncuesta.Publicada);
                return new EncuestaPendienteResponseDto(
                    e.Id,
                    e.Titulo,
                    version.Id,
                    version.Instrucciones,
                    version.Preguntas
                        .OrderBy(p => p.Orden)
                        .Select(p =>
                        {
                            var tipo = p.TipoRespuestaPregunta;
                            return new EncuestaPendientePreguntaResponseDto(
                                p.Id,
                                p.Texto,
                                tipo?.Codigo ?? string.Empty,
                                tipo?.InstruccionUi ?? string.Empty,
                                p.Obligatoria,
                                tipo?.MaxCaracteresRespuesta,
                                p.Opciones
                                    .OrderBy(o => o.Orden)
                                    .Select(o => new EncuestaPendienteOpcionResponseDto(o.Id, o.Texto))
                                    .ToList());
                        })
                        .ToList());
            })
            .ToList();
    }
}
