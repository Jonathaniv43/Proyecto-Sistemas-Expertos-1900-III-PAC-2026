using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.CerrarEncuesta;

/// <summary>HU-030: cierra una encuesta para que no reciba más respuestas ni ediciones (RN-70).</summary>
public sealed class CerrarEncuestaServicio(
    ICerrarEncuestaRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado> EjecutarAsync(Guid encuestaId, CancellationToken ct)
    {
        var encuesta = await repositorio.BuscarAsync(encuestaId, ct);
        if (encuesta is null)
            return Errores.EncuestaNoEncontrada;

        if (sesion.VinculoEmpresaId is not Guid vinculoId || encuesta.CreadaPorId != vinculoId)
            return Errores.SoloCreadorPuedeEditar;

        if (encuesta.Estado == EstadoEncuesta.Cerrada)
            return Errores.EncuestaCerrada;

        encuesta.Estado = EstadoEncuesta.Cerrada;
        encuesta.CerradaEn = await reloj.AhoraAsync(ct);

        await unidadDeTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
