using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Domain.Entities.Documentos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Documentos.GenerarDocumento;

/// <summary>Plantilla que se puede usar para generar un documento.</summary>
public sealed record PlantillaParaGenerar(Guid Id, string Titulo, TipoDocumento TipoDocumento, string Contenido);

public interface IGenerarDocumentoRepositorio
{
    /// <summary>Los datos del perfil que llenan los marcadores, con el rubro; nulo si la empresa no existe.</summary>
    Task<(DatosEmpresaDocumento Datos, short? RubroId)?> DatosEmpresaAsync(Guid empresaId, CancellationToken ct);

    /// <summary>La plantilla, solo si está activa y es del rubro indicado o de todos los rubros.</summary>
    Task<PlantillaParaGenerar?> PlantillaDisponibleAsync(Guid plantillaId, short rubroId, CancellationToken ct);

    /// <summary>Agrega el documento con su primera versión; se guarda con la unidad de trabajo.</summary>
    void Agregar(Documento documento);

    Task<DocumentoDetalleResponseDto> DetalleAsync(Guid documentoId, CancellationToken ct);
}
