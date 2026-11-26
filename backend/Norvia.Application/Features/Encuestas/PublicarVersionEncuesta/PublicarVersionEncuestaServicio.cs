using Norvia.Application.Common.Auditoria;
using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Encuestas.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.PublicarVersionEncuesta;

/// <summary>
/// HU-030: publica una versión en borrador y marca la versión publicada anterior como reemplazada
/// en una sola transacción (D8, RN-12, RN-70).
/// </summary>
public sealed class PublicarVersionEncuestaServicio(
    IPublicarVersionEncuestaRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    ITenantActual sesion,
    Auditor auditor) : IServicioCasoDeUso
{
    public async Task<Resultado<EncuestaVersionResponseDto>> EjecutarAsync(
        Guid encuestaId, Guid versionId, CancellationToken ct)
    {
        var encuesta = await repositorio.BuscarConVersionesAsync(encuestaId, ct);
        if (encuesta is null)
            return Errores.EncuestaNoEncontrada;

        if (sesion.VinculoEmpresaId is not Guid vinculoId || encuesta.CreadaPorId != vinculoId)
            return Errores.SoloCreadorPuedeEditar;

        if (encuesta.Estado == EstadoEncuesta.Cerrada)
            return Errores.EncuestaCerrada;

        var objetivo = encuesta.Versiones.FirstOrDefault(v => v.Id == versionId);
        if (objetivo is null)
            return Errores.VersionNoEncontrada;

        if (objetivo.Estado != EstadoVersionEncuesta.Borrador)
            return Errores.VersionNoPublicable;

        if (objetivo.Preguntas.Count == 0)
            return Errores.EncuestaSinPreguntas;

        var ahora = await reloj.AhoraAsync(ct);

        var anterior = encuesta.Versiones.FirstOrDefault(v => v.Estado == EstadoVersionEncuesta.Publicada);
        if (anterior is not null)
            anterior.Estado = EstadoVersionEncuesta.Reemplazada;

        objetivo.Estado = EstadoVersionEncuesta.Publicada;
        objetivo.PublicadaEn = ahora;
        encuesta.Estado = EstadoEncuesta.Publicada;

        auditor.Registrar(AccionesAuditoria.EncuestaPublicada, EntidadesAuditoria.Encuesta, encuesta.Id, new
        {
            titulo = encuesta.Titulo,
            versionId = objetivo.Id,
            numeroVersion = objetivo.NumeroVersion,
            versionReemplazadaId = anterior?.Id
        });
        await unidadDeTrabajo.GuardarCambiosAsync(ct);
        return EncuestaVersionResponseDto.De(objetivo);
    }
}
