using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Empresas.RegistrarEmpresa;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Empresas;

namespace Norvia.Infrastructure.Persistence.Repositorios.Empresas;

public sealed class RegistrarEmpresaRepositorio(NorviaDbContext db) : IRegistrarEmpresaRepositorio
{
    public Task<bool> ExisteRtnAsync(string rtn, CancellationToken cancellationToken) =>
        db.Empresa.AnyAsync(e => e.RTN == rtn, cancellationToken);

    public Task<bool> ExisteEmailAsync(string emailNormalizado, CancellationToken cancellationToken) =>
        db.Acceso.AnyAsync(a => a.Email == emailNormalizado, cancellationToken);

    public void Agregar(Empresa empresa, Persona persona, Usuario usuario, Acceso acceso, VinculoEmpresa vinculo)
    {
        db.Empresa.Add(empresa);
        db.Persona.Add(persona);
        db.Usuario.Add(usuario);
        db.Acceso.Add(acceso);
        db.VinculoEmpresa.Add(vinculo);
    }
}