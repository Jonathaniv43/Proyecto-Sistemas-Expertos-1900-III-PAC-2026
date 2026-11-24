using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.SolicitarCambioContrasena;
using Norvia.Domain.Entities.Acceso;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Auth;

public sealed class SolicitarCambioContrasenaRepositorio(NorviaDbContext db) : ISolicitarCambioContrasenaRepositorio
{
    public Task<Acceso?> BuscarAccesoPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken) =>
        db.Acceso.AsNoTracking().FirstOrDefaultAsync(a => a.Email == emailNormalizado, cancellationToken);

    // Petición anónima: sin empresa de sesión, así que se ignora el filtro de vínculos.
    public Task<string?> BuscarPrimerNombreAsync(Guid accesoId, CancellationToken cancellationToken) =>
        db.VinculoEmpresa.IgnoreQueryFilters().AsNoTracking()
            .Where(v => v.AccesoId == accesoId)
            .OrderBy(v => v.CreadoEn)
            .Select(v => v.Persona.PrimerNombre)
            .FirstOrDefaultAsync(cancellationToken);
}
