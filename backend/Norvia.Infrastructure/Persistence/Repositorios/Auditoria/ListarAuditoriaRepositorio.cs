using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auditoria.ListarAuditoria;
using Norvia.Application.Features.Auditoria.ListarAuditoria.DTOs;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Auditoria;

// RegistroAuditoria es tabla de empresa: el filtro global limita a la empresa de la sesión (RN-05).
public sealed class ListarAuditoriaRepositorio(NorviaDbContext db) : IListarAuditoriaRepositorio
{
    public async Task<AuditoriaPaginadaResponseDto> ListarAsync(
        string? buscar,
        string? entidad,
        string? accion,
        DateTime? desdeUtc,
        DateTime? hastaUtcExclusivo,
        int pagina,
        int tamano,
        CancellationToken ct)
    {
        var consulta = db.RegistroAuditoria.AsNoTracking();

        if (!string.IsNullOrEmpty(entidad))
            consulta = consulta.Where(r => r.Entidad == entidad);

        if (!string.IsNullOrEmpty(accion))
            consulta = consulta.Where(r => r.Accion == accion);

        if (desdeUtc.HasValue)
            consulta = consulta.Where(r => r.Fecha >= desdeUtc.Value);

        if (hastaUtcExclusivo.HasValue)
            consulta = consulta.Where(r => r.Fecha < hastaUtcExclusivo.Value);

        if (!string.IsNullOrEmpty(buscar))
        {
            var patron = $"%{EscaparLike(buscar)}%";
            var idsPorDetalle = db.RegistroAuditoria
                .FromSqlInterpolated($@"SELECT * FROM ""RegistroAuditoria"" WHERE ""Detalle""::text ILIKE {patron} ESCAPE '!'")
                .Select(r => r.Id);

            consulta = consulta.Where(r =>
                EF.Functions.ILike(r.Accion, patron, "!")
                || EF.Functions.ILike(r.Entidad, patron, "!")
                || idsPorDetalle.Contains(r.Id)
                || (r.VinculoEmpresa != null && (
                    EF.Functions.ILike(r.VinculoEmpresa.Persona.PrimerNombre, patron, "!")
                    || (r.VinculoEmpresa.Persona.SegundoNombre != null && EF.Functions.ILike(r.VinculoEmpresa.Persona.SegundoNombre, patron, "!"))
                    || EF.Functions.ILike(r.VinculoEmpresa.Persona.PrimerApellido, patron, "!")
                    || (r.VinculoEmpresa.Persona.SegundoApellido != null && EF.Functions.ILike(r.VinculoEmpresa.Persona.SegundoApellido, patron, "!"))
                    || EF.Functions.ILike(r.VinculoEmpresa.Acceso.Email, patron, "!")))
                || (r.StaffPlataforma != null && EF.Functions.ILike(r.StaffPlataforma.Acceso.Email, patron, "!")));
        }

        var total = await consulta.CountAsync(ct);
        var totalPaginas = total == 0 ? 1 : (int)Math.Ceiling(total / (double)tamano);
        var salto = (pagina - 1) * tamano;

        var filas = await consulta
            .OrderByDescending(r => r.Fecha)
            .ThenByDescending(r => r.Id)
            .Skip(salto)
            .Take(tamano)
            .Select(r => new
            {
                r.Id,
                r.Fecha,
                r.Accion,
                r.Entidad,
                r.EntidadId,
                r.VinculoEmpresaId,
                r.StaffPlataformaId,
                PrimerNombre = r.VinculoEmpresa != null ? r.VinculoEmpresa.Persona.PrimerNombre : null,
                SegundoNombre = r.VinculoEmpresa != null ? r.VinculoEmpresa.Persona.SegundoNombre : null,
                PrimerApellido = r.VinculoEmpresa != null ? r.VinculoEmpresa.Persona.PrimerApellido : null,
                SegundoApellido = r.VinculoEmpresa != null ? r.VinculoEmpresa.Persona.SegundoApellido : null,
                CorreoEmpresa = r.VinculoEmpresa != null ? r.VinculoEmpresa.Acceso.Email : null,
                CorreoStaff = r.StaffPlataforma != null ? r.StaffPlataforma.Acceso.Email : null,
                r.Detalle
            })
            .ToListAsync(ct);

        var items = filas
            .Select(f =>
            {
                var actorTipo = f.VinculoEmpresaId.HasValue ? "empresa" : f.StaffPlataformaId.HasValue ? "plataforma" : "sistema";
                var actorNombre = f.VinculoEmpresaId.HasValue
                    ? string.Join(' ', new[] { f.PrimerNombre, f.SegundoNombre, f.PrimerApellido, f.SegundoApellido }
                        .Where(p => !string.IsNullOrWhiteSpace(p)))
                    : f.StaffPlataformaId.HasValue ? (f.CorreoStaff ?? "Equipo Norvia") : "Sistema";
                var actorCorreo = f.CorreoEmpresa ?? f.CorreoStaff;

                return new RegistroAuditoriaResponseDto(
                    f.Id,
                    f.Fecha,
                    f.Accion,
                    f.Entidad,
                    f.EntidadId,
                    actorTipo,
                    f.VinculoEmpresaId,
                    f.StaffPlataformaId,
                    actorNombre,
                    actorCorreo,
                    ParsearJson(f.Detalle));
            })
            .ToList();

        return new AuditoriaPaginadaResponseDto(items, pagina, tamano, total, totalPaginas);
    }

    private static JsonElement? ParsearJson(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        using var doc = JsonDocument.Parse(raw);
        return doc.RootElement.Clone();
    }

    private static string EscaparLike(string valor) =>
        valor.Replace("!", "!!").Replace("%", "!%").Replace("_", "!_");
}
