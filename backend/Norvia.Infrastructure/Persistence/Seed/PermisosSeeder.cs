using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Norvia.Application.Common.Seguridad;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Seed;

/// <summary>
/// Sincroniza la tabla Permiso y las asignaciones a roles con <see cref="CatalogoPermisos"/>.
/// El código es la fuente de verdad: agrega lo que falta, actualiza lo que cambió y desactiva lo que salió.
/// </summary>
public sealed class PermisosSeeder(NorviaDbContext db, ILogger<PermisosSeeder> log)
{
    public async Task EjecutarAsync(CancellationToken ct)
    {
        if (!await db.Database.CanConnectAsync(ct))
            return;

        ValidarCatalogo();

        try
        {
            await SincronizarAsync(ct);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            log.LogWarning(ex, "PermisosSeeder omitido (¿falta migración?).");
        }
    }

    /// <summary>Detecta descuidos al editar el catálogo: códigos o pares verbo+ruta repetidos.</summary>
    private static void ValidarCatalogo()
    {
        var codigos = CatalogoPermisos.Todos
            .GroupBy(d => d.Codigo).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (codigos.Count > 0)
            throw new InvalidOperationException($"Códigos repetidos en CatalogoPermisos: {string.Join(", ", codigos)}.");

        var rutas = CatalogoPermisos.Todos
            .GroupBy(d => (d.Metodo.ToUpperInvariant(), RutaNormalizada.De(d.Ruta)))
            .Where(g => g.Count() > 1).Select(g => $"{g.Key.Item1} {g.Key.Item2}").ToList();
        if (rutas.Count > 0)
            throw new InvalidOperationException($"Rutas repetidas en CatalogoPermisos: {string.Join(", ", rutas)}.");
    }

    private async Task SincronizarAsync(CancellationToken ct)
    {
        // 1. Permisos: crear o actualizar según el catálogo (la clave es el Codigo).
        var permisos = await db.Permiso.ToDictionaryAsync(p => p.Codigo, ct);

        foreach (var d in CatalogoPermisos.Todos)
        {
            var metodo = d.Metodo.ToUpperInvariant();
            var ruta = RutaNormalizada.De(d.Ruta);

            if (permisos.TryGetValue(d.Codigo, out var p))
            {
                p.Nombre = d.Nombre;
                p.Alcance = d.Alcance;
                p.MetodoHttp = metodo;
                p.RutaPatron = ruta;
                p.Activo = true;
            }
            else
            {
                p = new Permiso
                {
                    Codigo = d.Codigo,
                    Nombre = d.Nombre,
                    Alcance = d.Alcance,
                    MetodoHttp = metodo,
                    RutaPatron = ruta
                };
                permisos[d.Codigo] = p;
                db.Permiso.Add(p);
            }
        }

        // Los que salieron del catálogo se desactivan; no se borran (las FK son Restrict).
        var vigentes = CatalogoPermisos.Todos.Select(d => d.Codigo).ToHashSet();
        foreach (var p in permisos.Values.Where(p => !vigentes.Contains(p.Codigo)))
            p.Activo = false;

        // 2. Asignaciones deseadas, separadas por alcance (RN-63 / RN-64).
        var roles = await db.Rol.ToDictionaryAsync(r => r.Codigo, r => r.Id, ct);
        var rolesStaff = await db.RolStaffPlataforma.ToDictionaryAsync(r => r.Codigo, r => r.Id, ct);

        var deseadoEmpresa = new HashSet<(short Rol, Guid Permiso)>();
        var deseadoStaff = new HashSet<(short Rol, Guid Permiso)>();

        foreach (var d in CatalogoPermisos.Todos)
        foreach (var codigo in d.Roles)
        {
            var permisoId = permisos[d.Codigo].Id;
            if (d.Alcance == AlcancePermiso.Empresa)
                deseadoEmpresa.Add((Buscar(roles, codigo), permisoId));
            else
                deseadoStaff.Add((Buscar(rolesStaff, codigo), permisoId));
        }

        // 3. Roles de empresa: quitar las asignaciones sobrantes y agregar las que faltan.
        var actualesEmpresa = await db.RolPermiso.ToListAsync(ct);
        db.RolPermiso.RemoveRange(
            actualesEmpresa.Where(x => !deseadoEmpresa.Contains((x.RolId, x.PermisoId))));
        foreach (var par in deseadoEmpresa.Where(par =>
                     !actualesEmpresa.Any(x => x.RolId == par.Rol && x.PermisoId == par.Permiso)))
            db.RolPermiso.Add(new RolPermiso { RolId = par.Rol, PermisoId = par.Permiso });

        // 4. Roles de staff: lo mismo.
        var actualesStaff = await db.RolStaffPlataformaPermiso.ToListAsync(ct);
        db.RolStaffPlataformaPermiso.RemoveRange(
            actualesStaff.Where(x => !deseadoStaff.Contains((x.RolStaffPlataformaId, x.PermisoId))));
        foreach (var par in deseadoStaff.Where(par =>
                     !actualesStaff.Any(x => x.RolStaffPlataformaId == par.Rol && x.PermisoId == par.Permiso)))
            db.RolStaffPlataformaPermiso.Add(
                new RolStaffPlataformaPermiso { RolStaffPlataformaId = par.Rol, PermisoId = par.Permiso });

        await db.SaveChangesAsync(ct);
    }

    private static short Buscar(Dictionary<string, short> roles, string codigo) =>
        roles.TryGetValue(codigo, out var id)
            ? id
            : throw new InvalidOperationException($"El rol '{codigo}' del catálogo de permisos no existe.");
}