using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Empresas.RegistrarEmpresaMicrosoft;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Empresas;

public sealed class RegistrarEmpresaMicrosoftRepositorio(NorviaDbContext db) : IRegistrarEmpresaMicrosoftRepositorio
{
    public Task<bool> ExisteRtnAsync(string rtn, CancellationToken cancellationToken) =>
        db.Empresa.AnyAsync(e => e.RTN == rtn, cancellationToken);

    public Task<bool> ExisteEmailAsync(string emailNormalizado, CancellationToken cancellationToken) =>
        db.Acceso.AnyAsync(a => a.Email == emailNormalizado, cancellationToken);

    public Task<bool> ExisteDocumentoAsync(TipoDocumentoIdentidad tipoDocumento, string numeroDocumento, CancellationToken cancellationToken) =>
        db.Persona.AnyAsync(p => p.TipoDocumento == tipoDocumento && p.NumeroDocumento == numeroDocumento, cancellationToken);

    public Task<short?> BuscarRubroIdActivoAsync(string codigo, CancellationToken cancellationToken) =>
        db.Rubro
            .Where(r => r.Codigo == codigo && r.Activo)
            .Select(r => (short?)r.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Acceso?> BuscarAccesoPorProveedorAsync(
        ProveedorIdentidad proveedor, string proveedorUserId, CancellationToken cancellationToken) =>
        db.AccesoProveedor
            .Where(p => p.Proveedor == proveedor && p.ProveedorUserId == proveedorUserId)
            .Select(p => p.Acceso)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Persona?> BuscarPersonaDeAccesoAsync(Guid accesoId, CancellationToken cancellationToken) =>
        // IgnoreQueryFilters: aún no hay sesión de empresa. Exige el AccesoId ya validado con el proveedor,
        // así que solo devuelve la persona de esa cuenta. Rastreada: RN-85 completa sus datos vacíos.
        db.VinculoEmpresa.IgnoreQueryFilters()
            .Where(v => v.AccesoId == accesoId)
            .OrderBy(v => v.CreadoEn)
            .Select(v => v.Persona)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> UsuarioActivoAsync(Guid personaId, CancellationToken cancellationToken) =>
        db.Usuario.AnyAsync(u => u.PersonaId == personaId && u.Activo, cancellationToken);

    public void AgregarTodo(Empresa empresa, Persona persona, Usuario usuario, Acceso acceso, AccesoProveedor proveedor, VinculoEmpresa vinculo)
    {
        db.Empresa.Add(empresa);
        db.Persona.Add(persona);
        db.Usuario.Add(usuario);
        db.Acceso.Add(acceso);
        db.AccesoProveedor.Add(proveedor);
        db.VinculoEmpresa.Add(vinculo);
    }

    public void AgregarEmpresaYVinculo(Empresa empresa, VinculoEmpresa vinculo)
    {
        db.Empresa.Add(empresa);
        db.VinculoEmpresa.Add(vinculo);
    }
}
