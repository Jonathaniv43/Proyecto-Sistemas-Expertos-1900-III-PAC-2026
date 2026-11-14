using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Riesgos;

namespace Norvia.Infrastructure.Persistence;

/// <summary>
/// Contexto de EF Core de Norvia. Los DbSet van en singular porque EF usa su nombre
/// como nombre de tabla y el modelo de datos usa tablas en singular.
/// </summary>
public class NorviaDbContext(DbContextOptions<NorviaDbContext> options) : DbContext(options)
{
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
    public DbSet<Area> Area => Set<Area>();

    // Catálogos (globales)
    public DbSet<Rubro> Rubro => Set<Rubro>();
    public DbSet<CategoriaRiesgo> CategoriaRiesgo => Set<CategoriaRiesgo>();
    public DbSet<Peligro> Peligro => Set<Peligro>();
    public DbSet<PreguntaExploratoria> PreguntaExploratoria => Set<PreguntaExploratoria>();
    public DbSet<OpcionPregunta> OpcionPregunta => Set<OpcionPregunta>();

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
    }
}
