using System.Net;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Encuestas;

/// <summary>HU-029: el Responsable SST ve las categorías del rubro de su empresa, con la vista previa de las preguntas.</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class CategoriasDisponiblesTests(NorviaApiFactory api)
{
    private const string Ruta = "/api/encuestas/categorias-disponibles";
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Muestra_solo_lo_activo_del_rubro_de_la_empresa_con_sus_preguntas_en_orden()
    {
        var rubro = await CrearRubroAsync();
        var (ergonomia, _) = await CrearCatalogoAsync(rubro);
        var empresa = await _escenario.CrearEmpresaAsync("A", rubro);
        var responsable = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");

        var r = await responsable.GetAsync(Ruta);

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var categorias = (await Escenario.Json(r)).EnumerateArray().ToList();
        var categoria = Assert.Single(categorias); // la inactiva no aparece
        Assert.Equal(ergonomia, categoria.GetProperty("id").GetGuid());

        var preguntas = categoria.GetProperty("preguntasPreview").EnumerateArray().ToList();
        Assert.Equal(["¿Levantas cargas?", "Comentarios"], preguntas.Select(p => p.GetProperty("texto").GetString()));
        Assert.Equal("seleccion_unica", preguntas[0].GetProperty("tipoRespuestaCodigo").GetString());
        Assert.Equal(["Sí", "No"], preguntas[0].GetProperty("opciones").EnumerateArray().Select(o => o.GetProperty("texto").GetString()));
        Assert.Equal("respuesta_breve", preguntas[1].GetProperty("tipoRespuestaCodigo").GetString());
        Assert.False(preguntas[1].GetProperty("obligatoria").GetBoolean());
    }

    [Fact]
    public async Task Cada_empresa_ve_las_categorias_de_su_propio_rubro()
    {
        var rubroA = await CrearRubroAsync();
        var rubroB = await CrearRubroAsync();
        var (categoriaA, _) = await CrearCatalogoAsync(rubroA);
        var (categoriaB, _) = await CrearCatalogoAsync(rubroB);
        var a = await _escenario.CrearEmpresaAsync("A", rubroA);
        var b = await _escenario.CrearEmpresaAsync("B", rubroB);

        var idsA = await IdsAsync(await _escenario.ClienteConRolAsync(a, "responsable_sst"));
        var idsB = await IdsAsync(await _escenario.ClienteConRolAsync(b, "responsable_sst"));

        Assert.Equal([categoriaA], idsA);
        Assert.Equal([categoriaB], idsB);
    }

    [Fact]
    public async Task Sin_rubro_en_el_perfil_responde_409()
    {
        var empresa = await _escenario.CrearEmpresaAsync("SinRubro", rubroId: null);
        var responsable = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");

        var r = await responsable.GetAsync(Ruta);

        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        Assert.Equal("encuestas.empresa_sin_rubro", (await Escenario.Json(r)).GetProperty("codigo").GetString());
    }

    [Theory]
    [InlineData("colaborador")]
    [InlineData("encuestado")]
    public async Task Otros_roles_de_la_empresa_no_tienen_acceso(string rol)
    {
        var empresa = await _escenario.CrearEmpresaAsync("A", await CrearRubroAsync());
        var cliente = await _escenario.ClienteConRolAsync(empresa, rol);

        Assert.Equal(HttpStatusCode.Forbidden, (await cliente.GetAsync(Ruta)).StatusCode);
    }

    [Fact]
    public async Task El_staff_de_plataforma_no_tiene_acceso()
    {
        var staff = await _escenario.ClienteStaffAsync();

        Assert.Equal(HttpStatusCode.Forbidden, (await staff.GetAsync(Ruta)).StatusCode);
    }

    // ---------- Datos del catálogo ----------

    /// <summary>Un rubro propio por prueba, para que el catálogo de una no afecte a otra.</summary>
    private async Task<short> CrearRubroAsync()
    {
        await using var db = api.NuevoContexto(empresaId: null);
        var id = (short)(await db.Rubro.MaxAsync(r => r.Id) + 1);
        db.Rubro.Add(new Rubro { Id = id, Codigo = $"rubro_{Guid.NewGuid():N}"[..30], Nombre = $"Rubro {Guid.NewGuid():N}" });
        await db.SaveChangesAsync();
        return id;
    }

    /// <summary>Una categoría activa con 3 preguntas (una inactiva) y una categoría inactiva.</summary>
    private async Task<(Guid Activa, Guid Inactiva)> CrearCatalogoAsync(short rubroId)
    {
        await using var db = api.NuevoContexto(empresaId: null);
        var tipos = await db.TipoRespuestaPregunta.ToDictionaryAsync(t => t.Codigo, t => t.Id);
        if (tipos.Count == 0)
        {
            var filas = new[]
            {
                new TipoRespuestaPreguntaCatalogo
                {
                    Id = Guid.CreateVersion7(),
                    Codigo = TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve,
                    Nombre = "Respuesta breve",
                    InstruccionUi = TipoRespuestaPreguntaCatalogo.InstruccionUiPorDefecto(TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve)
                },
                new TipoRespuestaPreguntaCatalogo
                {
                    Id = Guid.CreateVersion7(),
                    Codigo = TipoRespuestaPreguntaCatalogo.CodigoSeleccionUnica,
                    Nombre = "Selección única",
                    InstruccionUi = TipoRespuestaPreguntaCatalogo.InstruccionUiPorDefecto(TipoRespuestaPreguntaCatalogo.CodigoSeleccionUnica),
                    RequiereOpciones = true
                },
                new TipoRespuestaPreguntaCatalogo
                {
                    Id = Guid.CreateVersion7(),
                    Codigo = TipoRespuestaPreguntaCatalogo.CodigoSeleccionMultiple,
                    Nombre = "Selección múltiple",
                    InstruccionUi = TipoRespuestaPreguntaCatalogo.InstruccionUiPorDefecto(TipoRespuestaPreguntaCatalogo.CodigoSeleccionMultiple),
                    RequiereOpciones = true
                }
            };
            db.TipoRespuestaPregunta.AddRange(filas);
            await db.SaveChangesAsync();
            tipos = filas.ToDictionary(t => t.Codigo, t => t.Id);
        }

        var activa = new CategoriaRiesgo { RubroId = rubroId, Nombre = "Ergonomía" };
        var inactiva = new CategoriaRiesgo { RubroId = rubroId, Nombre = "Desactivada", Activa = false };
        var conOpciones = new PreguntaExploratoria
        {
            CategoriaRiesgo = activa, Texto = "¿Levantas cargas?",
            TipoRespuestaPreguntaId = tipos[TipoRespuestaPreguntaCatalogo.CodigoSeleccionUnica], Orden = 1
        };
        conOpciones.Opciones.Add(new OpcionPregunta { Texto = "No", Orden = 2 });
        conOpciones.Opciones.Add(new OpcionPregunta { Texto = "Sí", Orden = 1 });
        db.AddRange(activa, inactiva, conOpciones,
            new PreguntaExploratoria { CategoriaRiesgo = activa, Texto = "Comentarios", TipoRespuestaPreguntaId = tipos[TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve], Obligatoria = false, Orden = 3 },
            new PreguntaExploratoria { CategoriaRiesgo = activa, Texto = "Retirada", TipoRespuestaPreguntaId = tipos[TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve], Orden = 2, Activa = false },
            new PreguntaExploratoria { CategoriaRiesgo = inactiva, Texto = "No debe verse", TipoRespuestaPreguntaId = tipos[TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve], Orden = 1 });
        await db.SaveChangesAsync();
        return (activa.Id, inactiva.Id);
    }

    private static async Task<List<Guid>> IdsAsync(HttpClient cliente) =>
        (await Escenario.Json(await cliente.GetAsync(Ruta))).EnumerateArray().Select(c => c.GetProperty("id").GetGuid()).ToList();
}
