using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;
using Norvia.Infrastructure.Opciones;
using Norvia.Infrastructure.Persistence.Seed;
using Norvia.Tests.Infraestructura;
using Npgsql;

namespace Norvia.Tests.Catalogos;

/// <summary>
/// HU-097: catálogos iniciales. Corre sobre una base propia (en el mismo contenedor) porque las demás pruebas trabajan
/// con catálogos vacíos.
/// </summary>
[Collection(ColeccionApi.Nombre)]
public sealed class CatalogoSstTests(NorviaApiFactory api)
{
    [Fact]
    public async Task Carga_los_cuatro_rubros_con_categorias_preguntas_peligros_y_plantillas_sin_duplicar_al_repetir()
    {
        await using var db = await BaseNuevaAsync();
        await SembrarAsync(db);
        var antes = await ContarAsync(db);
        await SembrarAsync(db); // carga repetida

        Assert.Equal(antes, await ContarAsync(db));
        // Los códigos cumplen la regla de la administración de catálogos (HU-023).
        Assert.All(await db.Peligro.AsNoTracking().Select(p => p.Codigo).ToListAsync(), c => Assert.Matches("^[a-z0-9_]+$", c));
        Assert.True(antes.Peligros >= 15 && antes.Plantillas >= 60 && antes.Preguntas >= 30, antes.ToString());

        // Los cuatro rubros tienen categorías, y cada categoría tiene preguntas.
        var categorias = await db.CategoriaRiesgo.AsNoTracking().Include(c => c.Preguntas).ToListAsync();
        Assert.Equal(4, categorias.Select(c => c.RubroId).Distinct().Count());
        Assert.All(categorias, c => Assert.NotEmpty(c.Preguntas));

        // Los tres tipos de respuesta, con las reglas de D26 y RN-68.
        var preguntas = await db.PreguntaExploratoria.AsNoTracking().Include(p => p.TipoRespuestaPregunta).Include(p => p.Opciones).ToListAsync();
        Assert.Equal(3, preguntas.Select(p => p.TipoRespuestaPregunta.Codigo).Distinct().Count());
        foreach (var p in preguntas)
        {
            if (p.TipoRespuestaPregunta.Codigo == TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve)
            {
                Assert.Empty(p.Opciones);
                Assert.Null(p.PeligroId);
                continue;
            }
            Assert.InRange(p.Opciones.Count, 2, 20);
            Assert.Equal(p.Opciones.Count, p.Opciones.Select(o => o.Texto.ToLowerInvariant()).Distinct().Count());
            if (p.PeligroId is not null) Assert.Contains(p.Opciones, o => o.IndicaExposicion); // qué opción indica exposición
            else Assert.DoesNotContain(p.Opciones, o => o.IndicaExposicion);
        }

        // Cada peligro sugiere controles (HU-039) y al menos una tarea (HU-042).
        var plantillas = await db.PlantillaTarea.AsNoTracking().ToListAsync();
        foreach (var peligro in await db.Peligro.AsNoTracking().ToListAsync())
        {
            Assert.Contains(plantillas, t => t.PeligroId == peligro.Id && t.TipoControl != null);
            Assert.Contains(plantillas, t => t.PeligroId == peligro.Id && t.TipoControl == null);
        }
    }

    [Fact]
    public async Task Los_grupos_de_acceso_incluyen_al_encuestado_con_sus_funciones()
    {
        await using var db = await BaseNuevaAsync();
        await new PermisosSeeder(db, NullLogger<PermisosSeeder>.Instance).EjecutarAsync(CancellationToken.None);

        var codigos = await db.Rol.AsNoTracking().Select(r => r.Codigo).ToListAsync();
        Assert.Equal(new[] { "admin_empresa", "colaborador", "encuestado", "responsable_sst" }, codigos.Order());
        var encuestado = await db.Rol.AsNoTracking().SingleAsync(r => r.Codigo == "encuestado");
        Assert.True(await db.RolPermiso.AnyAsync(rp => rp.RolId == encuestado.Id));
    }

    [Fact]
    public async Task No_pisa_lo_que_el_staff_edito_ni_duplica_lo_que_cargo_a_mano()
    {
        await using var db = await BaseNuevaAsync();
        // Antes de la carga, el staff ya creó un peligro con un código del catálogo.
        db.Peligro.Add(new Peligro { Codigo = "fis_ruido", Nombre = "Ruido (versión del staff)", Clasificacion = Domain.Enums.ClasificacionPeligro.Fisico });
        await db.SaveChangesAsync();
        await SembrarAsync(db);

        // Edita y desactiva elementos cargados.
        var ruido = await db.Peligro.SingleAsync(p => p.Codigo == "fis_ruido");
        var pregunta = await db.PreguntaExploratoria.FirstAsync(p => p.PeligroId == ruido.Id);
        pregunta.Texto = "Texto editado por el staff";
        var plantilla = await db.PlantillaTarea.FirstAsync(p => p.PeligroId == ruido.Id);
        plantilla.Activa = false;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await SembrarAsync(db);

        Assert.Equal("Ruido (versión del staff)", (await db.Peligro.SingleAsync(p => p.Codigo == "fis_ruido")).Nombre);
        Assert.Equal("Texto editado por el staff", (await db.PreguntaExploratoria.SingleAsync(p => p.Id == pregunta.Id)).Texto);
        Assert.False((await db.PlantillaTarea.SingleAsync(p => p.Id == plantilla.Id)).Activa);
        Assert.Equal(1, await db.Peligro.CountAsync(p => p.Codigo == "fis_ruido"));
    }

    // ---------- Apoyo ----------

    private sealed record Conteo(int Categorias, int Preguntas, int Opciones, int Peligros, int Plantillas);

    private static async Task<Conteo> ContarAsync(NorviaDbContext db) => new(
        await db.CategoriaRiesgo.CountAsync(), await db.PreguntaExploratoria.CountAsync(), await db.OpcionPregunta.CountAsync(),
        await db.Peligro.CountAsync(), await db.PlantillaTarea.CountAsync());

    private static async Task SembrarAsync(NorviaDbContext db)
    {
        await new TiposRespuestaPreguntaSeeder(db, NullLogger<TiposRespuestaPreguntaSeeder>.Instance).EjecutarAsync(CancellationToken.None);
        await new CatalogoSstSeeder(db, Options.Create(new CatalogoSstOpciones()), NullLogger<CatalogoSstSeeder>.Instance)
            .EjecutarAsync(CancellationToken.None);
        db.ChangeTracker.Clear();
    }

    /// <summary>Una base vacía y migrada en el contenedor de las pruebas, solo para esta prueba.</summary>
    private async Task<NorviaDbContext> BaseNuevaAsync()
    {
        var cadena = new NpgsqlConnectionStringBuilder(api.CadenaConexion) { Database = $"catalogo_{Guid.NewGuid():N}" }.ConnectionString;
        var tenant = new TenantFijo(null);
        var db = new NorviaDbContext(new DbContextOptionsBuilder<NorviaDbContext>().UseNpgsql(cadena).Options, tenant);
        await db.Database.MigrateAsync();
        return db;
    }
}
