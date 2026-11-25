using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Application.Features.Documentos.EditarBorrador.DTOs;

namespace Norvia.Application.Features.Documentos.EditarBorrador;

/// <summary>
/// HU-071: guarda el contenido nuevo del borrador de un documento (D13).
/// <list type="bullet">
/// <item>Solo el borrador se edita (RN-50): si el documento no tiene uno, 409 <c>documentos.sin_borrador</c>.</item>
/// <item>La plantilla no cambia: el borrador es una copia propia del documento.</item>
/// <item>Los marcadores <c>{{…}}</c> ya no se reemplazan: eso pasa solo al generar (HU-070).</item>
/// </list>
/// </summary>
public sealed class EditarBorradorServicio(IEditarBorradorRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo)
    : IServicioCasoDeUso
{
    public async Task<Resultado<DocumentoDetalleResponseDto>> EjecutarAsync(
        Guid documentoId, EditarBorradorRequestDto request, CancellationToken ct)
    {
        if (await repositorio.BorradorAsync(documentoId, ct) is not { } borrador)
            return await repositorio.ExisteDocumentoAsync(documentoId, ct) ? Errores.SinBorrador : Errores.DocumentoNoEncontrado;

        // Los saltos de línea se guardan como \n, venga de donde venga el texto.
        var contenido = request.Contenido!.Replace("\r\n", "\n").Replace('\r', '\n').TrimEnd();
        if (borrador.Contenido != contenido)
        {
            borrador.Contenido = contenido;
            await unidadDeTrabajo.GuardarCambiosAsync(ct);
        }

        var detalle = await repositorio.DetalleAsync(documentoId, ct);
        return detalle with { PorCompletar = Marcadores.PorCompletar(detalle.Version.Contenido) };
    }
}
