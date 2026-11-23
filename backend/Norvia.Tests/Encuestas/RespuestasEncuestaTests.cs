using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Encuestas;

/// <summary>HU-031 (pendientes + responder) y HU-032 (reporte anónimo).</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class RespuestasEncuestaTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Pendientes_muestra_publicada_con_instrucciones_y_desaparece_al_responder()
    {
        var (empresa, sst, encuestaId, versionId, preguntas) = await PublicarEncuestaAsync();
        var encuestado = await _escenario.ClienteConRolAsync(empresa, "encuestado");
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");

        var pendientesEnc = await Escenario.Json(await encuestado.GetAsync("/api/encuestas/pendientes"));
        var item = pendientesEnc.EnumerateArray().Single(e => e.GetProperty("encuestaId").GetGuid() == encuestaId);
        Assert.Equal(versionId, item.GetProperty("versionId").GetGuid());
        Assert.Equal("Responde con sinceridad.", item.GetProperty("instrucciones").GetString());
        var preguntasDto = item.GetProperty("preguntas").EnumerateArray().ToList();
        Assert.Equal(2, preguntasDto.Count);
        Assert.False(string.IsNullOrEmpty(preguntasDto[0].GetProperty("instruccionUi").GetString()));
        Assert.Equal("seleccion_unica", preguntasDto[0].GetProperty("tipoRespuesta").GetString());

        Assert.Equal(HttpStatusCode.OK, (await colaborador.GetAsync("/api/encuestas/pendientes")).StatusCode);

        var unica = preguntas.Single(p => p.Tipo == "seleccion_unica");
        var breve = preguntas.Single(p => p.Tipo == "respuesta_breve");
        var responder = await encuestado.PostAsJsonAsync($"/api/encuestas/{encuestaId}/respuestas", new
        {
            versionId,
            respuestas = new object[]
            {
                new { preguntaId = unica.Id, opcionId = unica.OpcionIds[0] },
                new { preguntaId = breve.Id, textoLibre = "Comentario de prueba" }
            }
        });
        Assert.Equal(HttpStatusCode.OK, responder.StatusCode);
        Assert.Contains("registrada", (await Escenario.Json(responder)).GetProperty("mensaje").GetString(),
            StringComparison.OrdinalIgnoreCase);

        var pendientesTras = await Escenario.Json(await encuestado.GetAsync("/api/encuestas/pendientes"));
        Assert.DoesNotContain(pendientesTras.EnumerateArray(),
            e => e.GetProperty("encuestaId").GetGuid() == encuestaId);

        var segunda = await encuestado.PostAsJsonAsync($"/api/encuestas/{encuestaId}/respuestas", new
        {
            versionId,
            respuestas = new object[]
            {
                new { preguntaId = unica.Id, opcionId = unica.OpcionIds[0] }
            }
        });
        Assert.Equal(HttpStatusCode.Conflict, segunda.StatusCode);
        Assert.Equal("encuestas.ya_participo", (await Escenario.Json(segunda)).GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Responder_rechaza_version_obsoleta_y_obligatoria_faltante()
    {
        var (empresa, _, encuestaId, versionId, preguntas) = await PublicarEncuestaAsync();
        var encuestado = await _escenario.ClienteConRolAsync(empresa, "encuestado");
        var unica = preguntas.Single(p => p.Tipo == "seleccion_unica");

        var versionMala = await encuestado.PostAsJsonAsync($"/api/encuestas/{encuestaId}/respuestas", new
        {
            versionId = Guid.NewGuid(),
            respuestas = new object[]
            {
                new { preguntaId = unica.Id, opcionId = unica.OpcionIds[0] }
            }
        });
        Assert.Equal(HttpStatusCode.Conflict, versionMala.StatusCode);
        Assert.Equal("encuestas.version_obsoleta", (await Escenario.Json(versionMala)).GetProperty("codigo").GetString());

        var falta = await encuestado.PostAsJsonAsync($"/api/encuestas/{encuestaId}/respuestas", new
        {
            versionId,
            respuestas = new object[]
            {
                new { preguntaId = preguntas.Single(p => p.Tipo == "respuesta_breve").Id, textoLibre = "solo breve" }
            }
        });
        Assert.Equal(HttpStatusCode.BadRequest, falta.StatusCode);
        Assert.Equal("encuestas.respuesta_invalida", (await Escenario.Json(falta)).GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Reporte_oculta_detalle_con_menos_de_tres_y_muestra_con_tres()
    {
        var (empresa, sst, encuestaId, versionId, preguntas) = await PublicarEncuestaAsync();
        var unica = preguntas.Single(p => p.Tipo == "seleccion_unica");
        var breve = preguntas.Single(p => p.Tipo == "respuesta_breve");

        async Task ResponderComoAsync(string rol)
        {
            var cliente = await _escenario.ClienteConRolAsync(empresa, rol);
            var r = await cliente.PostAsJsonAsync($"/api/encuestas/{encuestaId}/respuestas", new
            {
                versionId,
                respuestas = new object[]
                {
                    new { preguntaId = unica.Id, opcionId = unica.OpcionIds[0] },
                    new { preguntaId = breve.Id, textoLibre = $"Texto {rol}" }
                }
            });
            Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        }

        await ResponderComoAsync("encuestado");
        await ResponderComoAsync("colaborador");

        var conDos = await Escenario.Json(await sst.GetAsync($"/api/encuestas/{encuestaId}/reporte"));
        Assert.False(conDos.GetProperty("detalleOculto").GetBoolean());
        Assert.Equal(2, conDos.GetProperty("totalRespuestasAnonimas").GetInt32());
        var distDos = conDos.GetProperty("preguntas").EnumerateArray()
            .Single(p => p.GetProperty("tipoRespuesta").GetString() == "seleccion_unica")
            .GetProperty("distribucion").EnumerateArray();
        Assert.Contains(distDos, d => d.GetProperty("cantidad").GetInt32() == 2);

        // Tercera participación: otro encuestado (roles distintos, un vínculo cada uno).
        await ResponderComoAsync("encuestado");

        var conTres = await Escenario.Json(await sst.GetAsync($"/api/encuestas/{encuestaId}/reporte"));
        Assert.False(conTres.GetProperty("detalleOculto").GetBoolean());
        Assert.Equal(3, conTres.GetProperty("totalRespuestasAnonimas").GetInt32());
        var preguntaUnica = conTres.GetProperty("preguntas").EnumerateArray()
            .Single(p => p.GetProperty("tipoRespuesta").GetString() == "seleccion_unica");
        var dist = preguntaUnica.GetProperty("distribucion").EnumerateArray().ToList();
        Assert.Contains(dist, d => d.GetProperty("cantidad").GetInt32() == 3);

        var preguntaBreve = conTres.GetProperty("preguntas").EnumerateArray()
            .Single(p => p.GetProperty("tipoRespuesta").GetString() == "respuesta_breve");
        Assert.Equal(3, preguntaBreve.GetProperty("textosBreves").GetArrayLength());

        var sinPermiso = await _escenario.ClienteConRolAsync(empresa, "encuestado");
        Assert.Equal(HttpStatusCode.Forbidden,
            (await sinPermiso.GetAsync($"/api/encuestas/{encuestaId}/reporte")).StatusCode);
    }

    private async Task<(
        EmpresaDePrueba Empresa,
        HttpClient Sst,
        Guid EncuestaId,
        Guid VersionId,
        List<PreguntaPublicada> Preguntas)> PublicarEncuestaAsync()
    {
        var rubro = await CrearRubroAsync();
        var categoriaId = await CrearCatalogoAsync(rubro);
        var empresa = await _escenario.CrearEmpresaAsync("Resp", rubro);
        var sst = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");

        var creada = await Escenario.Json(await sst.PostAsJsonAsync("/api/encuestas", new
        {
            titulo = "Diagnóstico respuesta",
            instrucciones = "Responde con sinceridad.",
            categoriaRiesgoIds = new[] { categoriaId }
        }));
        var encuestaId = creada.GetProperty("id").GetGuid();
        var versionId = creada.GetProperty("versionBorradorId").GetGuid();

        var pub = await sst.PostAsync($"/api/encuestas/{encuestaId}/versiones/{versionId}/publicacion", null);
        Assert.Equal(HttpStatusCode.OK, pub.StatusCode);

        var detalle = await Escenario.Json(await sst.GetAsync($"/api/encuestas/{encuestaId}"));
        var preguntasJson = detalle.GetProperty("versionPublicada").GetProperty("preguntas").EnumerateArray();
        var preguntas = preguntasJson.Select(p => new PreguntaPublicada(
            p.GetProperty("id").GetGuid(),
            p.GetProperty("tipoRespuestaCodigo").GetString()!,
            p.GetProperty("opciones").EnumerateArray().Select(o => o.GetProperty("id").GetGuid()).ToList()
        )).ToList();

        return (empresa, sst, encuestaId, versionId, preguntas);
    }

    private async Task<short> CrearRubroAsync()
    {
        await using var db = api.NuevoContexto(empresaId: null);
        var id = (short)(await db.Rubro.MaxAsync(r => r.Id) + 1);
        db.Rubro.Add(new Rubro
        {
            Id = id,
            Codigo = $"rubro_{Guid.NewGuid():N}"[..30],
            Nombre = $"Rubro {Guid.NewGuid():N}"
        });
        await db.SaveChangesAsync();
        return id;
    }

    private async Task<Guid> CrearCatalogoAsync(short rubroId)
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
            Texto = "¿Usas EPP?",
            TipoRespuestaPreguntaId = tipos[TipoRespuestaPreguntaCatalogo.CodigoSeleccionUnica],
            Peligro = peligro,
            Orden = 1
        };
        p1.Opciones.Add(new OpcionPregunta { Texto = "Siempre", Orden = 1, IndicaExposicion = false });
        p1.Opciones.Add(new OpcionPregunta { Texto = "Nunca", Orden = 2, IndicaExposicion = true });
        var p2 = new PreguntaExploratoria
        {
            CategoriaRiesgo = categoria,
            Texto = "Comentario",
            TipoRespuestaPreguntaId = tipos[TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve],
            Obligatoria = true,
            Orden = 2
        };
        db.AddRange(peligro, categoria, p1, p2);
        await db.SaveChangesAsync();
        return categoria.Id;
    }

    private static async Task<Dictionary<string, Guid>> AsegurarTiposRespuestaAsync(
        Norvia.Infrastructure.Data.NorviaDbContext db)
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
                InstruccionUi = TipoRespuestaPreguntaCatalogo.InstruccionUiPorDefecto(
                    TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve),
                MaxCaracteresRespuesta = 500
            },
            new TipoRespuestaPreguntaCatalogo
            {
                Id = Guid.CreateVersion7(),
                Codigo = TipoRespuestaPreguntaCatalogo.CodigoSeleccionUnica,
                Nombre = "Selección única",
                InstruccionUi = TipoRespuestaPreguntaCatalogo.InstruccionUiPorDefecto(
                    TipoRespuestaPreguntaCatalogo.CodigoSeleccionUnica),
                RequiereOpciones = true
            },
            new TipoRespuestaPreguntaCatalogo
            {
                Id = Guid.CreateVersion7(),
                Codigo = TipoRespuestaPreguntaCatalogo.CodigoSeleccionMultiple,
                Nombre = "Selección múltiple",
                InstruccionUi = TipoRespuestaPreguntaCatalogo.InstruccionUiPorDefecto(
                    TipoRespuestaPreguntaCatalogo.CodigoSeleccionMultiple),
                RequiereOpciones = true
            }
        };
        db.TipoRespuestaPregunta.AddRange(filas);
        await db.SaveChangesAsync();
        return filas.ToDictionary(t => t.Codigo, t => t.Id);
    }

    private sealed record PreguntaPublicada(Guid Id, string Tipo, IReadOnlyList<Guid> OpcionIds);
}
