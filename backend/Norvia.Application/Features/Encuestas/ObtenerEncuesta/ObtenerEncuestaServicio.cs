using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Encuestas.DTOs;
using Norvia.Application.Features.Encuestas.ObtenerEncuesta.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.ObtenerEncuesta;

public sealed class ObtenerEncuestaServicio(IObtenerEncuestaRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<EncuestaDetalleResponseDto>> EjecutarAsync(Guid id, CancellationToken ct)
    {
        var encuesta = await repositorio.BuscarAsync(id, ct);
        if (encuesta is null)
            return Errores.EncuestaNoEncontrada;

        var publicada = encuesta.Versiones.FirstOrDefault(v => v.Estado == EstadoVersionEncuesta.Publicada);
        var borrador = encuesta.Versiones.FirstOrDefault(v => v.Estado == EstadoVersionEncuesta.Borrador);

        return new EncuestaDetalleResponseDto(
            encuesta.Id,
            encuesta.Titulo,
            encuesta.Estado,
            encuesta.CreadaPorId,
            encuesta.CreadaEn,
            encuesta.CerradaEn,
            publicada is null ? null : EncuestaVersionResponseDto.De(publicada),
            borrador is null ? null : EncuestaVersionResponseDto.De(borrador));
    }
}
