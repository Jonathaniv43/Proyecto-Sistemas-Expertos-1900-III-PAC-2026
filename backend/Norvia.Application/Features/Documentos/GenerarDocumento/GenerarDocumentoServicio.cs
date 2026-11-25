using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Application.Features.Documentos.GenerarDocumento.DTOs;
using Norvia.Domain.Entities.Documentos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Documentos.GenerarDocumento;

/// <summary>
/// HU-070: genera una política o un procedimiento desde una plantilla de su rubro (D13).
/// <list type="bullet">
/// <item>Los marcadores se llenan con los datos del perfil; lo que falta queda como <c>[Completar: …]</c> y la respuesta
/// lo lista en <see cref="DocumentoDetalleResponseDto.PorCompletar"/> (RN-49).</item>
/// <item>Queda como versión 1 en borrador, para que la empresa revise su contenido antes de aprobarlo (RN-49).</item>
/// <item>La plantilla no cambia: el documento guarda su propia copia del texto.</item>
/// </list>
/// </summary>
public sealed class GenerarDocumentoServicio(
    IGenerarDocumentoRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado<DocumentoDetalleResponseDto>> EjecutarAsync(GenerarDocumentoRequestDto request, CancellationToken ct)
    {
        if (sesion.EmpresaId is not Guid empresaId || sesion.VinculoEmpresaId is not Guid vinculoId) return Errores.SinVinculo;
        if (await repositorio.DatosEmpresaAsync(empresaId, ct) is not { } empresa) return Errores.SinVinculo;
        if (empresa.RubroId is not short rubroId) return Errores.SinRubro;
        if (await repositorio.PlantillaDisponibleAsync(request.PlantillaDocumentoId!.Value, rubroId, ct) is not { } plantilla)
            return Errores.PlantillaNoDisponible;

        var hoy = HoyEnHonduras(await reloj.AhoraAsync(ct));
        var documento = new Documento
        {
            PlantillaDocumentoId = plantilla.Id,
            Titulo = string.IsNullOrWhiteSpace(request.Titulo) ? plantilla.Titulo : request.Titulo.Trim(),
            TipoDocumento = plantilla.TipoDocumento,
            CreadoPorId = vinculoId
        };
        documento.Versiones.Add(new VersionDocumento
        {
            DocumentoId = documento.Id,
            NumeroVersion = 1,
            Contenido = Marcadores.Reemplazar(plantilla.Contenido, empresa.Datos, hoy),
            Estado = EstadoVersionDocumento.Borrador,
            CreadaPorId = vinculoId
        });
        repositorio.Agregar(documento);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        var detalle = await repositorio.DetalleAsync(documento.Id, ct);
        return detalle with { PorCompletar = Marcadores.PorCompletar(detalle.Version.Contenido) };
    }

    private static DateOnly HoyEnHonduras(DateTime ahoraUtc) => DateOnly.FromDateTime(ahoraUtc.AddHours(-6));
}
