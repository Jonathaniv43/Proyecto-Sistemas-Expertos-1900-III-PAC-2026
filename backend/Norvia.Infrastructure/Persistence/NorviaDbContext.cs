using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Norvia.Application.Common.Interfaces;
using Norvia.Domain.Common;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;

namespace Norvia.Infrastructure.Persistence;

/// <summary>
/// Contexto de EF Core de Norvia. Los DbSet van en singular porque EF usa su nombre
/// como nombre de tabla y el modelo de datos usa tablas en singular.
/// <para>
/// Aislamiento multiempresa (§2, HU-093): toda entidad <see cref="ITieneEmpresa"/> tiene un filtro global
/// <c>EmpresaId == EmpresaIdActual</c>. Sin sesión de empresa, <see cref="EmpresaIdActual"/> es nulo y esas
/// tablas no devuelven filas. Saltar el filtro (<c>IgnoreQueryFilters</c>) solo se permite en el login,
/// los seeders y las operaciones de plataforma, con un comentario que lo justifique.
/// </para>
/// </summary>
public class NorviaDbContext(DbContextOptions<NorviaDbContext> options, ITenantActual tenant) : DbContext(options)
{
    /// <summary>Empresa de la sesión. EF la lee en cada consulta (no queda fija en el modelo).</summary>
    public Guid? EmpresaIdActual => tenant.EmpresaId;

    // Acceso (globales, salvo VinculoEmpresa e HistorialEstadoVinculo)
    public DbSet<Persona> Persona => Set<Persona>();
    public DbSet<Usuario> Usuario => Set<Usuario>();
    public DbSet<Acceso> Acceso => Set<Acceso>();
    public DbSet<RefreshToken> RefreshToken => Set<RefreshToken>();
    public DbSet<StaffPlataforma> StaffPlataforma => Set<StaffPlataforma>();
    public DbSet<RolStaffPlataforma> RolStaffPlataforma => Set<RolStaffPlataforma>();
    public DbSet<Rol> Rol => Set<Rol>();
    public DbSet<Permiso> Permiso => Set<Permiso>();
    public DbSet<RolPermiso> RolPermiso => Set<RolPermiso>();
    public DbSet<RolStaffPlataformaPermiso> RolStaffPlataformaPermiso => Set<RolStaffPlataformaPermiso>();
    public DbSet<VinculoEmpresa> VinculoEmpresa => Set<VinculoEmpresa>();
    public DbSet<HistorialEstadoVinculo> HistorialEstadoVinculo => Set<HistorialEstadoVinculo>();

    // Empresas
    public DbSet<Empresa> Empresa => Set<Empresa>();
    public DbSet<HistorialEstadoEmpresa> HistorialEstadoEmpresa => Set<HistorialEstadoEmpresa>();

    // Catálogos (globales)
    public DbSet<Rubro> Rubro => Set<Rubro>();
    public DbSet<CategoriaRiesgo> CategoriaRiesgo => Set<CategoriaRiesgo>();
    public DbSet<Peligro> Peligro => Set<Peligro>();
    public DbSet<PreguntaExploratoria> PreguntaExploratoria => Set<PreguntaExploratoria>();
    public DbSet<OpcionPregunta> OpcionPregunta => Set<OpcionPregunta>();
    public DbSet<PlantillaTarea> PlantillaTarea => Set<PlantillaTarea>();
    public DbSet<PlantillaDocumento> PlantillaDocumento => Set<PlantillaDocumento>();

    // Riesgos
    public DbSet<RiesgoEmpresa> RiesgoEmpresa => Set<RiesgoEmpresa>();
    public DbSet<EvaluacionRiesgo> EvaluacionRiesgo => Set<EvaluacionRiesgo>();
    public DbSet<Control> Control => Set<Control>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Carga todas las IEntityTypeConfiguration<T> de Persistence/Configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NorviaDbContext).Assembly);

        // Ninguna tabla se borra en cascada: todas las FK usan ON DELETE RESTRICT (modelo de datos §6).
        foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        }

        AplicarFiltroPorEmpresa(modelBuilder);
    }

    /// <summary>HasQueryFilter(e => e.EmpresaId == EmpresaIdActual) en cada entidad ITieneEmpresa.</summary>
    private void AplicarFiltroPorEmpresa(ModelBuilder modelBuilder)
    {
        var tiposDeEmpresa = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(ITieneEmpresa).IsAssignableFrom(t.ClrType) && t.BaseType is null);

        foreach (var tipo in tiposDeEmpresa)
        {
            var e = Expression.Parameter(tipo.ClrType, "e");
            var empresaDeLaFila = Expression.Convert(
                Expression.Property(e, nameof(ITieneEmpresa.EmpresaId)), typeof(Guid?));
            var empresaDeLaSesion = Expression.Property(Expression.Constant(this), nameof(EmpresaIdActual));
            var filtro = Expression.Lambda(Expression.Equal(empresaDeLaFila, empresaDeLaSesion), e);

            modelBuilder.Entity(tipo.ClrType).HasQueryFilter(filtro);
        }
    }
}
