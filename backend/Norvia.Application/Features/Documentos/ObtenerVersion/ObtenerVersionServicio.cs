using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Documentos.DTOs;

namespace Norvia.Application.Features.Documentos.ObtenerVersion;

/// <summary>HU-073: consulta de una versión concreta de un documento (vigente, reemplazada o borrador).</summary>
public sealed class ObtenerVersionServicio(IObtenerVersionRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<DocumentoDetalleResponseDto>> EjecutarAsync(
        Guid documentoId, Guid versionId, CancellationToken ct)
    {
        if (await repositorio.DetalleDeVersionAsync(documentoId, versionId, ct) is { } detalle)
            return detalle with { PorCompletar = Marcadores.PorCompletar(detalle.Version.Contenido) };

        return await repositorio.ExisteDocumentoAsync(documentoId, ct)
            ? Errores.VersionNoEncontrada
            : Errores.DocumentoNoEncontrado;
    }
}
