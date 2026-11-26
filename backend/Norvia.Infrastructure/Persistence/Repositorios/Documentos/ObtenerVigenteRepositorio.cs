using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Application.Features.Documentos.ObtenerVigente;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Documentos;

// VersionDocumento, Documento y VinculoEmpresa son tablas de empresa: el filtro global limita a la sesión.
public sealed class ObtenerVigenteRepositorio(NorviaDbContext db) : IObtenerVigenteRepositorio
{
    public async Task<DocumentoVigenteResponseDto?> VigenteAsync(Guid documentoId, CancellationToken ct)
    {
        var fila = await db.VersionDocumento.AsNoTracking()
            .Where(v => v.DocumentoId == documentoId && v.Estado == EstadoVersionDocumento.Vigente)
            .Select(v => new
            {
                v.DocumentoId, v.Documento.Titulo, v.Documento.TipoDocumento, v.NumeroVersion, v.Contenido,
                AprobadaEn = v.AprobadaEn!.Value, AprobadaPor = v.AprobadaPor!.Persona
            })
            .FirstOrDefaultAsync(ct);
        return fila is null
            ? null
            : new DocumentoVigenteResponseDto(fila.DocumentoId, fila.Titulo, fila.TipoDocumento, fila.NumeroVersion,
                fila.Contenido, fila.AprobadaEn, fila.AprobadaPor.NombreCompleto);
    }
}
