using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Empresas.RegistrarEmpresa;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Empresas;

public sealed class RegistrarEmpresaRepositorio(NorviaDbContext db) : IRegistrarEmpresaRepositorio
{
    public Task<bool> ExisteRtnAsync(string rtn, CancellationToken cancellationToken) =>
        db.Empresa.AnyAsync(e => e.RTN == rtn, cancellationToken);

    public Task<bool> ExisteEmailAsync(string emailNormalizado, CancellationToken cancellationToken) =>
        db.Acceso.AnyAsync(a => a.Email == emailNormalizado, cancellationToken);

    public Task<bool> ExisteDocumentoAsync(TipoDocumentoIdentidad tipo, string numeroDocumento, CancellationToken cancellationToken) =>
        db.Persona.AnyAsync(p => p.TipoDocumento == tipo && p.NumeroDocumento == numeroDocumento, cancellationToken);

    public Task<short?> BuscarRubroIdActivoAsync(string codigo, CancellationToken cancellationToken) =>
        db.Rubro
            .Where(r => r.Codigo == codigo && r.Activo)
            .Select(r => (short?)r.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public void Agregar(Empresa empresa, Persona persona, Usuario usuario, Acceso acceso, VinculoEmpresa vinculo)
    {
        db.Empresa.Add(empresa);
        db.Persona.Add(persona);
        db.Usuario.Add(usuario);
        db.Acceso.Add(acceso);
        db.VinculoEmpresa.Add(vinculo);
    }
}