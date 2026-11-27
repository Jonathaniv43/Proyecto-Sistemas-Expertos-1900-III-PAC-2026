using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Documentos.DTOs;

namespace Norvia.Application.Features.Documentos.ListarDocumentos;

/// <summary>HU-070: las políticas y los procedimientos de la empresa, con el estado de su versión de trabajo.</summary>
public sealed class ListarDocumentosServicio(IListarDocumentosRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<IReadOnlyList<DocumentoResumenResponseDto>>> EjecutarAsync(CancellationToken ct) =>
        Resultado<IReadOnlyList<DocumentoResumenResponseDto>>.Exito(await repositorio.ListarAsync(ct));
}
