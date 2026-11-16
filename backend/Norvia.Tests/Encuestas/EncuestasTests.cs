using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Encuestas;

/// <summary>
/// HU-030: creación, edición por borrador, versionado al publicar, cierre y control de autoría/roles.
/// </summary>
[Collection(ColeccionApi.Nombre)]
public sealed class EncuestasTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Crear_encuesta_copia_preguntas_activas_del_rubro_y_no_cambia_si_cambia_el_catalogo()
    {
        var rubro = await CrearRubroAsync();
        var (categoriaId, preguntaActivaId, _) = await CrearCatalogoAsync(rubro);
        var empresa = await _escenario.CrearEmpresaAsync("EncA", rubro);
        var sst = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");

        var crear = await sst.PostAsJsonAsync("/api/encuestas", new
        {
            titulo = "Diagnóstico SST 2026",
            instrucciones = "Responde con sinceridad.",
            categoriaRiesgoIds = new[] { categoriaId }
        });

        Assert.Equal(HttpStatusCode.Created, crear.StatusCode);
        var creada = await Escenario.Json(crear);
        var encuestaId = creada.GetProperty("id").GetGuid();
        Assert.Equal("borrador", creada.GetProperty("estado").GetString());
        Assert.NotEqual(Guid.Empty, creada.GetProperty("versionBorradorId").GetGuid());
        Assert.Equal(System.Text.Json.JsonValueKind.Null, creada.GetProperty("versionPublicadaId").ValueKind);

        // Cambiar el catálogo después no debe alterar la encuesta ya creada (RN-12).
        await using (var db = api.NuevoContexto(empresaId: null))
        {
            var origen = await db.PreguntaExploratoria.SingleAsync(p => p.Id == preguntaActivaId);
            origen.Texto = "Pregunta modificada en catálogo";
            await db.SaveChangesAsync();
        }

        var detalle = await Escenario.Json(await sst.GetAsync($"/api/encuestas/{encuestaId}"));
        var borrador = detalle.GetProperty("versionBorrador");
        var secciones = borrador.GetProperty("secciones").EnumerateArray().ToList();
        Assert.Single(secciones);
        Assert.False(string.IsNullOrWhiteSpace(secciones[0].GetProperty("titulo").GetString()));
        var preguntas = borrador.GetProperty("preguntas").EnumerateArray().ToList();
        Assert.Equal(["¿Levantas cargas pesadas?", "Observaciones"],
            preguntas.Select(p => p.GetProperty("texto").GetString()));
        Assert.False(string.IsNullOrEmpty(preguntas[0].GetProperty("instruccionUi").GetString()));
        Assert.Equal("seleccion_unica", preguntas[0].GetProperty("tipoRespuestaCodigo").GetString());
        Assert.Equal(secciones[0].GetProperty("id").GetGuid(), preguntas[0].GetProperty("seccionId").GetGuid());
    }

    [Fact]
    public async Task Crear_encuesta_rechaza_empresa_sin_rubro_o_categoria_invalida()
    {
        var empresaSinRubro = await _escenario.CrearEmpresaAsync("SinRubro", rubroId: null);
        var sstSinRubro = await _escenario.ClienteConRolAsync(empresaSinRubro, "responsable_sst");
        var rSinRubro = await sstSinRubro.PostAsJsonAsync("/api/encuestas", new
        {
            titulo = "Intento",
            categoriaRiesgoIds = new[] { Guid.NewGuid() }
        });
        Assert.Equal(HttpStatusCode.Conflict, rSinRubro.StatusCode);
        Assert.Equal("encuestas.empresa_sin_rubro", (await Escenario.Json(rSinRubro)).GetProperty("codigo").GetString());

        var rubroEmpresa = await CrearRubroAsync();
        var otroRubro = await CrearRubroAsync();
        var (categoriaOtroRubro, _, _) = await CrearCatalogoAsync(otroRubro);
        var empresa = await _escenario.CrearEmpresaAsync("ConRubro", rubroEmpresa);
        var sst = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");

        var rOtra = await sst.PostAsJsonAsync("/api/encuestas", new
        {
            titulo = "Intento",
            categoriaRiesgoIds = new[] { categoriaOtroRubro }
        });
        Assert.Equal(HttpStatusCode.BadRequest, rOtra.StatusCode);
        Assert.Equal("encuestas.categoria_invalida", (await Escenario.Json(rOtra)).GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Publicar_y_editar_encuesta_publicada_crea_version_nueva_sin_alterar_la_vigente()
    {
        var rubro = await CrearRubroAsync();
        var (categoriaId, preguntaOrigenId, peligroId) = await CrearCatalogoAsync(rubro);
        var empresa = await _escenario.CrearEmpresaAsync("Versiones", rubro);
        var sst = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");

        var creada = await Escenario.Json(await sst.PostAsJsonAsync("/api/encuestas", new
        {
            titulo = "Encuesta Inicial",
            categoriaRiesgoIds = new[] { categoriaId }
        }));
        var encuestaId = creada.GetProperty("id").GetGuid();
        var v1Id = creada.GetProperty("versionBorradorId").GetGuid();

        // Editar borrador v1 antes de publicar.
        var detalleInicial = await Escenario.Json(await sst.GetAsync($"/api/encuestas/{encuestaId}"));
        var preguntaCopiadaId = detalleInicial.GetProperty("versionBorrador").GetProperty("preguntas")[0].GetProperty("id").GetGuid();

        var edicionV1 = await sst.PutAsJsonAsync($"/api/encuestas/{encuestaId}/borrador", new
        {
            titulo = "Encuesta Editada v1",
            instrucciones = "Instrucciones v1",
            secciones = Array.Empty<object>(),
            preguntas = new object[]
            {
                new
                {
                    id = preguntaCopiadaId,
                    texto = "¿Manipulas cargas mayores a 20 kg?",
                    tipoRespuestaCodigo = "seleccion_unica",
                    obligatoria = true,
                    peligroId,
                    orden = 1,
                    opciones = new object[]
                    {
                        new { texto = "Sí", orden = 1, indicaExposicion = true },
                        new { texto = "No", orden = 2, indicaExposicion = false }
                    }
                },
                new
                {
                    id = (Guid?)null,
                    texto = "Pregunta personalizada",
                    tipoRespuestaCodigo = "respuesta_breve",
                    obligatoria = false,
                    peligroId = (Guid?)null,
                    orden = 2,
                    opciones = Array.Empty<object>()
                }
            }
        });
        Assert.Equal(HttpStatusCode.OK, edicionV1.StatusCode);
        var v1Editada = await Escenario.Json(edicionV1);
        Assert.Equal(1, v1Editada.GetProperty("numeroVersion").GetInt32());
        Assert.Equal(preguntaOrigenId, v1Editada.GetProperty("preguntas")[0].GetProperty("preguntaExploratoriaId").GetGuid());
        Assert.Equal(System.Text.Json.JsonValueKind.Null, v1Editada.GetProperty("preguntas")[1].GetProperty("preguntaExploratoriaId").ValueKind);

        // Publicar v1.
        var pub1 = await sst.PostAsync($"/api/encuestas/{encuestaId}/versiones/{v1Id}/publicacion", null);
        Assert.Equal(HttpStatusCode.OK, pub1.StatusCode);
        var v1Pub = await Escenario.Json(pub1);
        Assert.Equal("publicada", v1Pub.GetProperty("estado").GetString());

        // Editar estando publicada: crea borrador v2 sin modificar v1 vigente.
        var edicionV2 = await sst.PutAsJsonAsync($"/api/encuestas/{encuestaId}/borrador", new
        {
            instrucciones = "Instrucciones v2",
            secciones = Array.Empty<object>(),
            preguntas = new object[]
            {
                new
                {
                    id = (Guid?)null,
                    texto = "Nueva pregunta v2",
                    tipoRespuestaCodigo = "respuesta_breve",
                    obligatoria = true,
                    peligroId = (Guid?)null,
                    orden = 1,
                    opciones = Array.Empty<object>()
                }
            }
        });
        Assert.Equal(HttpStatusCode.OK, edicionV2.StatusCode);
        var v2Borrador = await Escenario.Json(edicionV2);
        var v2Id = v2Borrador.GetProperty("id").GetGuid();
        Assert.Equal(2, v2Borrador.GetProperty("numeroVersion").GetInt32());
        Assert.Equal("borrador", v2Borrador.GetProperty("estado").GetString());

        var detalleConDos = await Escenario.Json(await sst.GetAsync($"/api/encuestas/{encuestaId}"));
        Assert.Equal(1, detalleConDos.GetProperty("versionPublicada").GetProperty("numeroVersion").GetInt32());
        Assert.Equal("Instrucciones v1", detalleConDos.GetProperty("versionPublicada").GetProperty("instrucciones").GetString());
        Assert.Equal(2, detalleConDos.GetProperty("versionBorrador").GetProperty("numeroVersion").GetInt32());

        // Publicar v2: v1 pasa a reemplazada y v2 a publicada.
        var pub2 = await sst.PostAsync($"/api/encuestas/{encuestaId}/versiones/{v2Id}/publicacion", null);
        Assert.Equal(HttpStatusCode.OK, pub2.StatusCode);

        await using var dbCheck = api.NuevoContexto(empresa.EmpresaId);
        var estados = await dbCheck.EncuestaVersion
            .Where(v => v.EncuestaId == encuestaId)
            .OrderBy(v => v.NumeroVersion)
            .Select(v => new { v.NumeroVersion, v.Estado })
            .ToListAsync();
        Assert.Equal(EstadoVersionEncuesta.Reemplazada, estados[0].Estado);
        Assert.Equal(EstadoVersionEncuesta.Publicada, estados[1].Estado);

        // Listado devuelve la versión publicada actual (2).
        var lista = await Escenario.Json(await sst.GetAsync("/api/encuestas"));
        var item = Assert.Single(lista.EnumerateArray());
        Assert.Equal(2, item.GetProperty("versionPublicadaNumero").GetInt32());
    }

    [Fact]
    public async Task Solo_quien_creo_la_encuesta_puede_editarla_publicarla_o_cerrarla()
    {
        var rubro = await CrearRubroAsync();
        var (categoriaId, _, _) = await CrearCatalogoAsync(rubro);
        var empresa = await _escenario.CrearEmpresaAsync("Autoria", rubro);
        var creador = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");
        var otroSst = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");

        var creada = await Escenario.Json(await creador.PostAsJsonAsync("/api/encuestas", new
        {
            titulo = "Encuesta del creador",
            categoriaRiesgoIds = new[] { categoriaId }
        }));
        var encuestaId = creada.GetProperty("id").GetGuid();
        var versionId = creada.GetProperty("versionBorradorId").GetGuid();

        // Otro Responsable SST de la misma empresa sí puede verla, pero no modificarla.
        Assert.Equal(HttpStatusCode.OK, (await otroSst.GetAsync($"/api/encuestas/{encuestaId}")).StatusCode);

        var rEditar = await otroSst.PutAsJsonAsync($"/api/encuestas/{encuestaId}/borrador", new
        {
            secciones = Array.Empty<object>(),
            preguntas = new object[]
            {
                new { texto = "Intento ajeno", tipoRespuestaCodigo = "respuesta_breve", obligatoria = true, orden = 1, opciones = Array.Empty<object>() }
            }
        });
        Assert.Equal(HttpStatusCode.Forbidden, rEditar.StatusCode);
        Assert.Equal("encuestas.solo_creador_puede_editar", (await Escenario.Json(rEditar)).GetProperty("codigo").GetString());

        var rPublicar = await otroSst.PostAsync($"/api/encuestas/{encuestaId}/versiones/{versionId}/publicacion", null);
        Assert.Equal(HttpStatusCode.Forbidden, rPublicar.StatusCode);

        var rCerrar = await otroSst.PostAsync($"/api/encuestas/{encuestaId}/cierre", null);
        Assert.Equal(HttpStatusCode.Forbidden, rCerrar.StatusCode);
    }

    [Fact]
    public async Task Cerrar_encuesta_impide_ediciones_y_publicaciones_posteriores()
    {
        var rubro = await CrearRubroAsync();
        var (categoriaId, _, _) = await CrearCatalogoAsync(rubro);
        var empresa = await _escenario.CrearEmpresaAsync("Cierre", rubro);
        var sst = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");

        var creada = await Escenario.Json(await sst.PostAsJsonAsync("/api/encuestas", new
        {
            titulo = "Encuesta a cerrar",
            categoriaRiesgoIds = new[] { categoriaId }
        }));
        var encuestaId = creada.GetProperty("id").GetGuid();
        var versionId = creada.GetProperty("versionBorradorId").GetGuid();

        Assert.Equal(HttpStatusCode.NoContent, (await sst.PostAsync($"/api/encuestas/{encuestaId}/cierre", null)).StatusCode);

        var editar = await sst.PutAsJsonAsync($"/api/encuestas/{encuestaId}/borrador", new
        {
            secciones = Array.Empty<object>(),
            preguntas = new object[]
            {
                new { texto = "Cambio tardío", tipoRespuestaCodigo = "respuesta_breve", obligatoria = true, orden = 1, opciones = Array.Empty<object>() }
            }
        });
        Assert.Equal(HttpStatusCode.Conflict, editar.StatusCode);
        Assert.Equal("encuestas.cerrada", (await Escenario.Json(editar)).GetProperty("codigo").GetString());

        var publicar = await sst.PostAsync($"/api/encuestas/{encuestaId}/versiones/{versionId}/publicacion", null);
        Assert.Equal(HttpStatusCode.Conflict, publicar.StatusCode);
    }

    [Theory]
    [InlineData("colaborador")]
    [InlineData("encuestado")]
    public async Task Otros_roles_no_pueden_crear_ni_listar_encuestas(string rol)
    {
        var empresa = await _escenario.CrearEmpresaAsync("Roles", await CrearRubroAsync());
        var cliente = await _escenario.ClienteConRolAsync(empresa, rol);

        Assert.Equal(HttpStatusCode.Forbidden, (await cliente.GetAsync("/api/encuestas")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await cliente.PostAsJsonAsync("/api/encuestas", new
        {
            titulo = "No autorizado",
            categoriaRiesgoIds = new[] { Guid.NewGuid() }
        })).StatusCode);
    }

    [Fact]
    public async Task Admin_empresa_puede_listar_y_ver_tipos_respuesta()
    {
        var rubro = await CrearRubroAsync();
        var empresa = await _escenario.CrearEmpresaAsync("AdminEnc", rubro);
        var admin = empresa.Cliente;

        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/api/encuestas")).StatusCode);

        var tipos = await admin.GetAsync("/api/encuestas/tipos-respuesta");
        Assert.Equal(HttpStatusCode.OK, tipos.StatusCode);
        var lista = (await Escenario.Json(tipos)).EnumerateArray().ToList();
        Assert.Contains(lista, t => t.GetProperty("codigo").GetString() == "respuesta_breve");
        Assert.False(string.IsNullOrEmpty(lista[0].GetProperty("instruccionUi").GetString()));
    }

    // ---------- Helpers de catálogo ----------

    private async Task<short> CrearRubroAsync()
    {
        await using var db = api.NuevoContexto(empresaId: null);
        var id = (short)(await db.Rubro.MaxAsync(r => r.Id) + 1);
        db.Rubro.Add(new Rubro { Id = id, Codigo = $"rubro_{Guid.NewGuid():N}"[..30], Nombre = $"Rubro {Guid.NewGuid():N}" });
        await db.SaveChangesAsync();
        return id;
    }

    private async Task<(Guid CategoriaId, Guid PreguntaActivaId, Guid PeligroId)> CrearCatalogoAsync(short rubroId)
    {
        await using var db = api.NuevoContexto(empresaId: null);
        var tipos = await AsegurarTiposRespuestaAsync(db);
        var peligro = new Peligro
        {
            Codigo = $"pel_{Guid.NewGuid():N}"[..20],
            Nombre = $"Peligro {Guid.NewGuid():N}"[..30],
            Clasificacion = ClasificacionPeligro.Ergonomico
        };
        var categoria = new CategoriaRiesgo { RubroId = rubroId, Nombre = $"Cat {Guid.NewGuid():N}"[..25] };
        var p1 = new PreguntaExploratoria
        {
            CategoriaRiesgo = categoria,
            Texto = "¿Levantas cargas pesadas?",
            TipoRespuestaPreguntaId = tipos[TipoRespuestaPreguntaCatalogo.CodigoSeleccionUnica],
            Peligro = peligro,
            Orden = 1
        };
        p1.Opciones.Add(new OpcionPregunta { Texto = "Sí", Orden = 1, IndicaExposicion = true });
        p1.Opciones.Add(new OpcionPregunta { Texto = "No", Orden = 2, IndicaExposicion = false });

        var p2 = new PreguntaExploratoria
        {
            CategoriaRiesgo = categoria,
            Texto = "Observaciones",
            TipoRespuestaPreguntaId = tipos[TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve],
            Obligatoria = false,
            Orden = 2
        };
        var pInactiva = new PreguntaExploratoria
        {
            CategoriaRiesgo = categoria,
            Texto = "Inactiva",
            TipoRespuestaPreguntaId = tipos[TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve],
            Activa = false,
            Orden = 3
        };

        db.AddRange(peligro, categoria, p1, p2, pInactiva);
        await db.SaveChangesAsync();
        return (categoria.Id, p1.Id, peligro.Id);
    }

    private static async Task<Dictionary<string, Guid>> AsegurarTiposRespuestaAsync(Norvia.Infrastructure.Data.NorviaDbContext db)
    {
        var existentes = await db.TipoRespuestaPregunta.ToDictionaryAsync(t => t.Codigo, t => t.Id);
        if (existentes.Count > 0) return existentes;

        var filas = new[]
        {
            new TipoRespuestaPreguntaCatalogo
            {
                Id = Guid.CreateVersion7(),
                Codigo = TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve,
                Nombre = "Respuesta breve",
                InstruccionUi = TipoRespuestaPreguntaCatalogo.InstruccionUiPorDefecto(TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve),
                MaxCaracteresRespuesta = 500
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
        return filas.ToDictionary(t => t.Codigo, t => t.Id);
    }
}
