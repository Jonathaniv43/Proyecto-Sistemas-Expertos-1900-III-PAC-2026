using Norvia.Application.Common.Auditoria;
using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Notificaciones;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Documentos.AprobarDocumento;

/// <summary>
/// HU-072: aprueba el borrador de un documento y lo deja vigente (D13).
/// <list type="bullet">
/// <item>Registra quién lo aprobó y cuándo. Desde ahí la versión no se edita (RN-50).</item>
/// <item>Si el documento ya tenía una versión vigente, pasa a reemplazada: una sola vigente por documento (RN-51).</item>
/// <item>No se aprueba mientras queden datos por completar (RN-49): la versión aprobada ya no cambia.</item>
/// <item>Si es una política, avisa a cada vínculo activo de la empresa en Norvia y por correo (RN-75, HU-116). El aviso
/// se guarda con la aprobación y el correo sale después, con reintentos (RN-78).</item>
/// </list>
/// </summary>
public sealed class AprobarDocumentoServicio(
    IAprobarDocumentoRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    ITenantActual sesion,
    Notificador notificador,
    Auditor auditor) : IServicioCasoDeUso
{
    public async Task<Resultado<DocumentoDetalleResponseDto>> EjecutarAsync(Guid documentoId, CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid vinculoId) return Errores.SinVinculo;
        if (await repositorio.BorradorAsync(documentoId, ct) is not { } borrador)
            return await repositorio.ExisteDocumentoAsync(documentoId, ct) ? Errores.SinBorrador : Errores.DocumentoNoEncontrado;
        if (Marcadores.PorCompletar(borrador.Contenido).Count > 0) return Errores.PorCompletar;

        var anterior = await repositorio.VigenteAsync(documentoId, ct);
        var aprobadaEn = await reloj.AhoraAsync(ct);
        var destinatarios = borrador.Documento.TipoDocumento == TipoDocumento.Politica
            ? await repositorio.VinculosActivosAsync(ct)
            : [];

        // La base solo admite una vigente por documento (RN-51) y revisa cada UPDATE por separado: la anterior deja de
        // ser vigente antes de que el borrador lo sea. Los dos pasos se confirman juntos.
        await unidadDeTrabajo.EnTransaccionAsync(async t =>
        {
            if (anterior is not null)
            {
                anterior.Estado = EstadoVersionDocumento.Reemplazada;
                await unidadDeTrabajo.GuardarCambiosAsync(t);
            }
            borrador.Estado = EstadoVersionDocumento.Vigente;
            borrador.AprobadaPorId = vinculoId;
            borrador.AprobadaEn = aprobadaEn;
            notificador.PoliticaPublicada(borrador.Id, borrador.Documento.Titulo, destinatarios);
            auditor.Registrar(AccionesAuditoria.DocumentoAprobado, EntidadesAuditoria.Documento, borrador.DocumentoId, new
            {
                titulo = borrador.Documento.Titulo,
                tipoDocumento = borrador.Documento.TipoDocumento,
                versionId = borrador.Id,
                numeroVersion = borrador.NumeroVersion,
                versionReemplazadaId = anterior?.Id
            });
            await unidadDeTrabajo.GuardarCambiosAsync(t);
        }, ct);

        return await repositorio.DetalleAsync(documentoId, ct);
    }
}
