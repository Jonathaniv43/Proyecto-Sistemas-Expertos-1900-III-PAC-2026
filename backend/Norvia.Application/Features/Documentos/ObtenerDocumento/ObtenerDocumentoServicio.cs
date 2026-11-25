using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Documentos.DTOs;

namespace Norvia.Application.Features.Documentos.ObtenerDocumento;

/// <summary>
/// HU-070: un documento con su versión de trabajo (el borrador si hay uno; si no, la vigente) y lo que falta completar.
/// De otra empresa responde 404, igual que si no existiera.
/// </summary>
public sealed class ObtenerDocumentoServicio(IObtenerDocumentoRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<DocumentoDetalleResponseDto>> EjecutarAsync(Guid id, CancellationToken ct) =>
        await repositorio.DetalleAsync(id, ct) is { } detalle
            ? detalle with { PorCompletar = Marcadores.PorCompletar(detalle.Version.Contenido) }
            : Errores.DocumentoNoEncontrado;
}
