using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Domain.Entities.Documentos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Documentos.CrearVersionNueva;

/// <summary>
/// HU-073: crea una versión nueva en borrador copiando el contenido de la versión vigente (RN-50, D13).
/// La versión vigente sigue publicada hasta que esta nueva versión se apruebe (HU-072).
/// </summary>
public sealed class CrearVersionNuevaServicio(
    ICrearVersionNuevaRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado<DocumentoDetalleResponseDto>> EjecutarAsync(Guid documentoId, CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid vinculoId) return Errores.SinVinculo;
        if (!await repositorio.ExisteDocumentoAsync(documentoId, ct)) return Errores.DocumentoNoEncontrado;
        if (await repositorio.TieneBorradorAsync(documentoId, ct)) return Errores.YaTieneBorrador;
        if (await repositorio.VigenteAsync(documentoId, ct) is not { } vigente) return Errores.SinVigente;

        var numeroSiguiente = (await repositorio.MaximoNumeroVersionAsync(documentoId, ct)) + 1;
        var nueva = new VersionDocumento
        {
            DocumentoId = documentoId,
            NumeroVersion = numeroSiguiente,
            Contenido = vigente.Contenido,
            Estado = EstadoVersionDocumento.Borrador,
            CreadaPorId = vinculoId
        };

        repositorio.Agregar(nueva);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        var detalle = await repositorio.DetalleAsync(documentoId, ct);
        return detalle with { PorCompletar = Marcadores.PorCompletar(detalle.Version.Contenido) };
    }
}
