using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Documentos;
using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Application.Features.Documentos.GenerarDocumento;
using Norvia.Domain.Entities.Documentos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Documentos;

// Documento y VersionDocumento son tablas de empresa: EmpresaId se asigna al guardar. Empresa (raíz) se busca por el
// EmpresaId de la sesión, y PlantillaDocumento es global.
public sealed class GenerarDocumentoRepositorio(NorviaDbContext db) : IGenerarDocumentoRepositorio
{
    public async Task<(DatosEmpresaDocumento Datos, short? RubroId)?> DatosEmpresaAsync(Guid empresaId, CancellationToken ct)
    {
        var e = await db.Empresa.AsNoTracking()
            .Where(x => x.Id == empresaId)
            .Select(x => new { x.RazonSocial, x.RTN, x.Direccion, x.ActividadEconomica, x.RubroId, Rubro = x.Rubro != null ? x.Rubro.Nombre : null })
            .FirstOrDefaultAsync(ct);
        return e is null ? null : (new DatosEmpresaDocumento(e.RazonSocial, e.RTN, e.Direccion, e.ActividadEconomica, e.Rubro), e.RubroId);
    }

    public Task<PlantillaParaGenerar?> PlantillaDisponibleAsync(Guid plantillaId, short rubroId, CancellationToken ct) =>
        db.PlantillaDocumento.AsNoTracking()
            .Where(p => p.Id == plantillaId && p.Activa && (p.RubroId == null || p.RubroId == rubroId))
            .Select(p => new PlantillaParaGenerar(p.Id, p.Titulo, p.TipoDocumento, p.Contenido))
            .FirstOrDefaultAsync(ct);

    public void Agregar(Documento documento) => db.Documento.Add(documento);

    public async Task<DocumentoDetalleResponseDto> DetalleAsync(Guid documentoId, CancellationToken ct) =>
        (await ProyeccionDocumento.DetalleAsync(db, documentoId, ct))!;
}
