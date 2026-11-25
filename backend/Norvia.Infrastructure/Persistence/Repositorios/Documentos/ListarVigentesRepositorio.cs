using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Documentos.DTOs;
using Norvia.Application.Features.Documentos.ListarVigentes;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Documentos;

// VersionDocumento y Documento son tablas de empresa: el filtro global limita a la sesión.
public sealed class ListarVigentesRepositorio(NorviaDbContext db) : IListarVigentesRepositorio
{
    public async Task<IReadOnlyList<DocumentoVigenteResumenResponseDto>> ListarAsync(string? buscar, CancellationToken ct)
    {
        var q = db.VersionDocumento.AsNoTracking().Where(v => v.Estado == EstadoVersionDocumento.Vigente);
        if (buscar is not null)
        {
            var t = buscar.ToLower();
            q = q.Where(v => v.Documento.Titulo.ToLower().Contains(t));
        }
        return await q
            .OrderBy(v => v.Documento.TipoDocumento).ThenBy(v => v.Documento.Titulo)
            .Select(v => new DocumentoVigenteResumenResponseDto(
                v.DocumentoId, v.Documento.Titulo, v.Documento.TipoDocumento, v.NumeroVersion, v.AprobadaEn!.Value))
            .ToListAsync(ct);
    }
}
