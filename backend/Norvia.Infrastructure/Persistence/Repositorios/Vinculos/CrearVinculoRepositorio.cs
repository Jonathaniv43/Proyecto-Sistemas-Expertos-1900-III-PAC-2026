using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Vinculos.CrearVinculo;
using Norvia.Domain.Entities.Acceso;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Vinculos;

public sealed class CrearVinculoRepositorio(NorviaDbContext db) : ICrearVinculoRepositorio
{
    public Task<Rol?> BuscarRolAsync(string codigo, CancellationToken ct) =>
        db.Rol.FirstOrDefaultAsync(r => r.Codigo == codigo, ct);

    public Task<Acceso?> BuscarAccesoAsync(string emailNormalizado, CancellationToken ct) =>
        db.Acceso.FirstOrDefaultAsync(a => a.Email == emailNormalizado, ct);

    // IgnoreQueryFilters: la persona es global y puede venir de un vínculo con otra empresa (RN-02).
    // Solo se lee su identidad; no se expone ningún dato de esa otra empresa.
    public Task<Persona?> BuscarPersonaDelAccesoAsync(Guid accesoId, CancellationToken ct) =>
        db.VinculoEmpresa.IgnoreQueryFilters()
            .Where(v => v.AccesoId == accesoId)
            .OrderBy(v => v.CreadoEn)
            .Select(v => v.Persona)
            .FirstOrDefaultAsync(ct);

    // Con el filtro global: solo mira la empresa de la sesión.
    public Task<bool> TieneVinculoEnEmpresaActualAsync(Guid accesoId, CancellationToken ct) =>
        db.VinculoEmpresa.AnyAsync(v => v.AccesoId == accesoId, ct);

    public void AgregarPersona(Persona persona, Usuario usuario)
    {
        db.Persona.Add(persona);
        db.Usuario.Add(usuario);
    }

    public void AgregarAcceso(Acceso acceso) => db.Acceso.Add(acceso);

    public void AgregarVinculo(VinculoEmpresa vinculo) => db.VinculoEmpresa.Add(vinculo);
}
