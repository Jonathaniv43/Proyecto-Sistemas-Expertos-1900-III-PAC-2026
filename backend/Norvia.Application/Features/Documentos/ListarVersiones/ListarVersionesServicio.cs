using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Documentos.DTOs;

namespace Norvia.Application.Features.Documentos.ListarVersiones;

/// <summary>HU-073: historial de versiones de un documento, de la más reciente a la primera.</summary>
public sealed class ListarVersionesServicio(IListarVersionesRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<IReadOnlyList<VersionResumenResponseDto>>> EjecutarAsync(Guid documentoId, CancellationToken ct)
    {
        if (!await repositorio.ExisteDocumentoAsync(documentoId, ct)) return Errores.DocumentoNoEncontrado;
        var versiones = await repositorio.ListarAsync(documentoId, ct);
        return Resultado<IReadOnlyList<VersionResumenResponseDto>>.Exito(versiones);
    }
}
