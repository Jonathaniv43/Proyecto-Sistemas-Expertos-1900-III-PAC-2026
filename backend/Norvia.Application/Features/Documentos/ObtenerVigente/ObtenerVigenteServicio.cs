using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Documentos.DTOs;

namespace Norvia.Application.Features.Documentos.ObtenerVigente;

/// <summary>
/// HU-074: la versión vigente de un documento, para cualquier persona con vínculo. Si solo tiene un borrador, responde
/// 404 igual que si no existiera: el borrador no se publica.
/// </summary>
public sealed class ObtenerVigenteServicio(IObtenerVigenteRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<DocumentoVigenteResponseDto>> EjecutarAsync(Guid id, CancellationToken ct) =>
        await repositorio.VigenteAsync(id, ct) is { } vigente ? vigente : Errores.DocumentoNoEncontrado;
}
