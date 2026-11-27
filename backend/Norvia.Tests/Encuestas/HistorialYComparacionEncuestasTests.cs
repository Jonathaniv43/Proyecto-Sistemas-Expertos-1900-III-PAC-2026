using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Encuestas;

/// <summary>
/// HU-034: historial de encuestas cerradas/con versiones anteriores, reportes por versión y comparación
/// de resultados y peligros sugeridos respetando RN-12, D8, RN-72 y RN-73.
/// </summary>
[Collection(ColeccionApi.Nombre)]
public sealed class HistorialYComparacionEncuestasTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Historial_incluye_encuestas_cerradas_y_con_versiones_reemplazadas_y_filtra_por_texto()
    {
        var rubro = await CrearRubroAsync();
        var (catErgo, catQuim, pelX, _, _) = await CrearCatalogoAsync(rubro);
        var empresa = await _escenario.CrearEmpresaAsync("Hist", rubro);
        var sst = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");

        // 1. Solo borrador (excluida).
        var borradorId = await CrearEncuestaViaApiAsync(sst, "Encuesta Borrador", catErgo);

        // 2. Publicada v1 sin cerrar ni reemplazar (excluida por defecto, incluida con incluirVigentes=true).
        var vigenteId = await CrearEncuestaViaApiAsync(sst, "Vigente Actual", catErgo);
        await PublicarPrimeraVersionAsync(sst, vigenteId);

        // 3. Publicada con v1 reemplazada + v2 vigente (incluida en historial).
        var conVersionesId = await CrearEncuestaViaApiAsync(sst, "Con Reemplazo", catQuim);
        var v1Reemp = await PublicarPrimeraVersionAsync(sst, conVersionesId);
        await ResponderViaDbAsync(empresa.EmpresaId, v1Reemp, 2, true);
        var v2Pub = await PublicarNuevaVersionViaApiAsync(sst, conVersionesId, "Pregunta v2", pelX, false);
        await ResponderViaDbAsync(empresa.EmpresaId, v2Pub, 1, false);

        // 4. Cerrada (incluida en historial).
        var cerradaId = await CrearEncuestaViaApiAsync(sst, "Diagnóstico 2025", catErgo);
        var vCerrada = await PublicarPrimeraVersionAsync(sst, cerradaId);
        await ResponderViaDbAsync(empresa.EmpresaId, vCerrada, 4, true);
        Assert.Equal(HttpStatusCode.NoContent, (await sst.PostAsync($"/api/encuestas/{cerradaId}/cierre", null)).StatusCode);

        // Consulta por defecto.
        var rHist = await sst.GetAsync("/api/encuestas/historial");
        Assert.Equal(HttpStatusCode.OK, rHist.StatusCode);
        var historial = (await Escenario.Json(rHist)).EnumerateArray().ToList();
        var ids = historial.Select(x => x.GetProperty("id").GetGuid()).ToList();
        Assert.DoesNotContain(borradorId, ids);
        Assert.DoesNotContain(vigenteId, ids);
        Assert.Contains(conVersionesId, ids);
        Assert.Contains(cerradaId, ids);

        var itemCerrada = Assert.Single(historial, x => x.GetProperty("id").GetGuid() == cerradaId);
        Assert.Equal("cerrada", itemCerrada.GetProperty("estado").GetString());
        Assert.NotEqual(JsonValueKind.Null, itemCerrada.GetProperty("cerradaEn").ValueKind);
        Assert.Equal(4, itemCerrada.GetProperty("resumen").GetProperty("respuestasAnonimas").GetInt32());
        Assert.NotEmpty(itemCerrada.GetProperty("categorias").EnumerateArray());

        var itemVersiones = Assert.Single(historial, x => x.GetProperty("id").GetGuid() == conVersionesId);
        Assert.Equal(2, itemVersiones.GetProperty("versiones").GetArrayLength());
        Assert.Equal(3, itemVersiones.GetProperty("resumen").GetProperty("respuestasAnonimas").GetInt32());

        // Búsqueda por título y por nombre de categoría.
        var porTitulo = (await Escenario.Json(await sst.GetAsync("/api/encuestas/historial?buscar=2025"))).EnumerateArray().ToList();
        Assert.Single(porTitulo, x => x.GetProperty("id").GetGuid() == cerradaId);

        var nombreCatQuim = await ObtenerNombreCategoriaAsync(catQuim);
        var porCat = (await Escenario.Json(
            await sst.GetAsync($"/api/encuestas/historial?buscar={Uri.EscapeDataString(nombreCatQuim)}"))).EnumerateArray().ToList();
        Assert.Single(porCat, x => x.GetProperty("id").GetGuid() == conVersionesId);

        // incluirVigentes=true expone también Vigente Actual para poder compararla.
        var conVigentes = (await Escenario.Json(await sst.GetAsync("/api/encuestas/historial?incluirVigentes=true")))
            .EnumerateArray().Select(x => x.GetProperty("id").GetGuid()).ToList();
        Assert.Contains(vigenteId, conVigentes);
        Assert.DoesNotContain(borradorId, conVigentes);

        // Filtro excedido (> 100 caracteres).
        var largo = new string('a', 101);
        Assert.Equal(HttpStatusCode.BadRequest, (await sst.GetAsync($"/api/encuestas/historial?buscar={largo}")).StatusCode);
    }

    [Fact]
    public async Task Reporte_con_versionId_devuelve_version_historica_y_rechaza_borrador()
    {
        var rubro = await CrearRubroAsync();
        var (catErgo, _, pelX, _, _) = await CrearCatalogoAsync(rubro);
        var empresa = await _escenario.CrearEmpresaAsync("RepVer", rubro);
        var sst = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");

        var encuestaId = await CrearEncuestaViaApiAsync(sst, "Encuesta Versionada", catErgo);
        var v1Id = await PublicarPrimeraVersionAsync(sst, encuestaId);
        await ResponderViaDbAsync(empresa.EmpresaId, v1Id, 3, elegirExposicion: true);

        var v2Id = await PublicarNuevaVersionViaApiAsync(sst, encuestaId, "¿Usas protección auditiva?", pelX, false);
        await ResponderViaDbAsync(empresa.EmpresaId, v2Id, 3, elegirExposicion: false);

        // Crear borrador v3 sin publicar (usando un id nuevo para la sección en la nueva versión).
        var detalleV2 = await Escenario.Json(await sst.GetAsync($"/api/encuestas/{encuestaId}"));
        var secV2 = detalleV2.GetProperty("versionPublicada").GetProperty("secciones")[0];
        var nuevaSeccionV3Id = Guid.CreateVersion7();
        var pregV2 = detalleV2.GetProperty("versionPublicada").GetProperty("preguntas")[0];
        var crearBorrador = await sst.PutAsJsonAsync($"/api/encuestas/{encuestaId}/borrador", new
        {
            titulo = "Encuesta Versionada",
            instrucciones = "Borrador v3",
            secciones = new[] { new { id = nuevaSeccionV3Id, titulo = secV2.GetProperty("titulo").GetString(), orden = (short)1 } },
            preguntas = new[]
            {
                new
                {
                    id = pregV2.GetProperty("id").GetGuid(),
                    seccionId = nuevaSeccionV3Id,
                    categoriaRiesgoId = catErgo,
                    texto = "Borrador v3",
                    tipoRespuestaCodigo = "seleccion_unica",
                    obligatoria = true,
                    peligroId = pelX,
                    orden = (short)1,
                    opciones = new[]
                    {
                        new { id = (Guid?)null, texto = "Sí", orden = (short)1, indicaExposicion = true },
                        new { id = (Guid?)null, texto = "No", orden = (short)2, indicaExposicion = false }
                    }
                }
            }
        });
        Assert.Equal(HttpStatusCode.OK, crearBorrador.StatusCode);
        var v3BorradorId = (await Escenario.Json(crearBorrador)).GetProperty("id").GetGuid();

        // Detalle incluye resumen de versiones publicadas/reemplazadas.
        var detalleConHist = await Escenario.Json(await sst.GetAsync($"/api/encuestas/{encuestaId}"));
        var versiones = detalleConHist.GetProperty("versionesPublicadas").EnumerateArray().ToList();
        Assert.Equal(2, versiones.Count);
        Assert.Equal(2, versiones[0].GetProperty("numeroVersion").GetInt32());
        Assert.Equal(1, versiones[1].GetProperty("numeroVersion").GetInt32());

        // Reporte histórico v1 (reemplazada): 100 % eligió opción de exposición ("Sí").
        var repV1 = await Escenario.Json(await sst.GetAsync($"/api/encuestas/{encuestaId}/reporte?versionId={v1Id}"));
        Assert.Equal(v1Id, repV1.GetProperty("versionId").GetGuid());
        Assert.Equal(1, repV1.GetProperty("numeroVersion").GetInt32());
        Assert.Equal(3, repV1.GetProperty("preguntas")[0].GetProperty("distribucion")[0].GetProperty("cantidad").GetInt32());

        // Reporte vigente por defecto (v2): 0 en "Sí", 3 en "No".
        var repV2 = await Escenario.Json(await sst.GetAsync($"/api/encuestas/{encuestaId}/reporte"));
        Assert.Equal(v2Id, repV2.GetProperty("versionId").GetGuid());
        Assert.Equal(0, repV2.GetProperty("preguntas")[0].GetProperty("distribucion")[0].GetProperty("cantidad").GetInt32());
        Assert.Equal(3, repV2.GetProperty("preguntas")[0].GetProperty("distribucion")[1].GetProperty("cantidad").GetInt32());

        // Borrador rechazado (409).
        var repBorrador = await sst.GetAsync($"/api/encuestas/{encuestaId}/reporte?versionId={v3BorradorId}");
        Assert.Equal(HttpStatusCode.Conflict, repBorrador.StatusCode);
    }

    [Fact]
    public async Task Comparar_dos_versiones_calcula_deltas_y_peligros_por_version()
    {
        var rubro = await CrearRubroAsync();
        var (catErgo, _, pelX, pelY, _) = await CrearCatalogoAsync(rubro);
        var empresa = await _escenario.CrearEmpresaAsync("CompVer", rubro);
        var sst = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");

        var encuestaId = await CrearEncuestaViaApiAsync(sst, "Comparativa Versiones", catErgo);
        var v1Id = await PublicarPrimeraVersionAsync(sst, encuestaId);
        // En v1: 4 respuestas -> 1 con exposición (25 %), 3 sin exposición (75 %).
        await ResponderViaDbAsync(empresa.EmpresaId, v1Id, 1, true);
        await ResponderViaDbAsync(empresa.EmpresaId, v1Id, 3, false);

        // Publicar v2 agregando pregunta exclusiva de peligro Y; 4 respuestas -> 3 con exposición en X (75 %).
        var v2Id = await PublicarNuevaVersionViaApiAsync(
            sst, encuestaId, "¿Levantas cargas pesadas? (revisada)", pelX, incluirSegundaPreguntaConPeligro: true, segundoPeligroId: pelY);
        await ResponderViaDbAsync(empresa.EmpresaId, v2Id, 3, true);
        await ResponderViaDbAsync(empresa.EmpresaId, v2Id, 1, false);

        // Confirmar peligro X en la encuesta (HU-035) para validar estado en comparativa.
        var sug = await Escenario.Json(await sst.GetAsync($"/api/encuestas/{encuestaId}/peligros-sugeridos"));
        var riesgoX = Assert.Single(sug.GetProperty("sugerencias").EnumerateArray(), s => s.GetProperty("peligroId").GetGuid() == pelX);
        var riesgoXId = riesgoX.GetProperty("riesgoSugeridoId").GetGuid();
        Assert.Equal(HttpStatusCode.OK, (await sst.PatchAsJsonAsync(
            $"/api/encuestas/{encuestaId}/peligros-sugeridos/{riesgoXId}",
            new { estado = "confirmado", actividad = "Almacén" })).StatusCode);

        var rComp = await sst.GetAsync($"/api/encuestas/comparacion?baseVersionId={v1Id}&compararVersionId={v2Id}");
        Assert.Equal(HttpStatusCode.OK, rComp.StatusCode);
        var comp = await Escenario.Json(rComp);

        Assert.False(comp.GetProperty("comparacionOculta").GetBoolean());
        var pComp = Assert.Single(comp.GetProperty("preguntasComparadas").EnumerateArray());
        Assert.Equal("catalogo", pComp.GetProperty("claveEmparejamiento").GetString());
        var opcionSi = Assert.Single(pComp.GetProperty("opciones").EnumerateArray(), o => o.GetProperty("etiqueta").GetString() == "Sí");
        Assert.Equal(25.0, opcionSi.GetProperty("porcentajeBase").GetDouble());
        Assert.Equal(75.0, opcionSi.GetProperty("porcentajeComparar").GetDouble());
        Assert.Equal(50.0, opcionSi.GetProperty("diferenciaPuntosPorcentuales").GetDouble());

        Assert.Single(comp.GetProperty("preguntasSoloComparar").EnumerateArray());

        var peligros = comp.GetProperty("peligrosComparados").EnumerateArray().ToList();
        var compPelX = Assert.Single(peligros, p => p.GetProperty("peligroId").GetGuid() == pelX);
        Assert.Equal(1, compPelX.GetProperty("respuestasBase").GetInt32());
        Assert.Equal(25.0, compPelX.GetProperty("porcentajeBase").GetDouble());
        Assert.Equal(3, compPelX.GetProperty("respuestasComparar").GetInt32());
        Assert.Equal(75.0, compPelX.GetProperty("porcentajeComparar").GetDouble());
        Assert.Equal(50.0, compPelX.GetProperty("diferenciaPuntosPorcentuales").GetDouble());
        Assert.Equal("confirmado", compPelX.GetProperty("estadoEnEncuestaBase").GetString());
    }

    [Fact]
    public async Task Comparar_encuestas_distintas_respeta_rn73_y_valida_parametros()
    {
        var rubro = await CrearRubroAsync();
        var (catErgo, _, _, _, _) = await CrearCatalogoAsync(rubro);
        var empresa = await _escenario.CrearEmpresaAsync("CompEnc", rubro);
        var sst = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");

        var e1 = await CrearEncuestaViaApiAsync(sst, "Encuesta A", catErgo);
        var v1 = await PublicarPrimeraVersionAsync(sst, e1);
        await ResponderViaDbAsync(empresa.EmpresaId, v1, 4, true);

        var e2 = await CrearEncuestaViaApiAsync(sst, "Encuesta B", catErgo);
        var v2 = await PublicarPrimeraVersionAsync(sst, e2);
        await ResponderViaDbAsync(empresa.EmpresaId, v2, 2, false);

        // Con 2 respuestas en B (< 3), RN-73 oculta el detalle comparativo.
        var oculta = await Escenario.Json(
            await sst.GetAsync($"/api/encuestas/comparacion?baseVersionId={v1}&compararVersionId={v2}"));
        Assert.True(oculta.GetProperty("comparacionOculta").GetBoolean());
        Assert.True(oculta.GetProperty("comparar").GetProperty("detalleOculto").GetBoolean());
        Assert.Empty(oculta.GetProperty("preguntasComparadas").EnumerateArray());
        Assert.Empty(oculta.GetProperty("peligrosComparados").EnumerateArray());

        // Al llegar a 3 respuestas en B, ya compara entre encuestas distintas.
        await ResponderViaDbAsync(empresa.EmpresaId, v2, 1, false);
        var visible = await Escenario.Json(
            await sst.GetAsync($"/api/encuestas/comparacion?baseVersionId={v1}&compararVersionId={v2}"));
        Assert.False(visible.GetProperty("comparacionOculta").GetBoolean());
        Assert.Single(visible.GetProperty("preguntasComparadas").EnumerateArray());

        // Misma versión -> 400 BadRequest.
        Assert.Equal(HttpStatusCode.BadRequest,
            (await sst.GetAsync($"/api/encuestas/comparacion?baseVersionId={v1}&compararVersionId={v1}")).StatusCode);
    }

    [Fact]
    public async Task Aislamiento_multitenant_y_permisos_de_roles()
    {
        var rubro = await CrearRubroAsync();
        var (catErgo, _, _, _, _) = await CrearCatalogoAsync(rubro);
        var empresaA = await _escenario.CrearEmpresaAsync("EmpA", rubro);
        var sstA = await _escenario.ClienteConRolAsync(empresaA, "responsable_sst");
        var adminA = await _escenario.ClienteConRolAsync(empresaA, "admin_empresa");
        var colabA = await _escenario.ClienteConRolAsync(empresaA, "colaborador");
        var encA = await _escenario.ClienteConRolAsync(empresaA, "encuestado");

        var empresaB = await _escenario.CrearEmpresaAsync("EmpB", rubro);
        var sstB = await _escenario.ClienteConRolAsync(empresaB, "responsable_sst");

        var eA1 = await CrearEncuestaViaApiAsync(sstA, "Privada A1", catErgo);
        var vA1 = await PublicarPrimeraVersionAsync(sstA, eA1);
        var eA2 = await CrearEncuestaViaApiAsync(sstA, "Privada A2", catErgo);
        var vA2 = await PublicarPrimeraVersionAsync(sstA, eA2);

        Assert.Equal(HttpStatusCode.OK, (await adminA.GetAsync("/api/encuestas/historial")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await adminA.GetAsync($"/api/encuestas/comparacion?baseVersionId={vA1}&compararVersionId={vA2}")).StatusCode);

        Assert.Equal(HttpStatusCode.Forbidden, (await colabA.GetAsync("/api/encuestas/historial")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await encA.GetAsync("/api/encuestas/historial")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await colabA.GetAsync($"/api/encuestas/comparacion?baseVersionId={vA1}&compararVersionId={vA2}")).StatusCode);

        // Otra empresa no ve encuestas ni versiones ajenas.
        var histB = await Escenario.Json(await sstB.GetAsync("/api/encuestas/historial?incluirVigentes=true"));
        Assert.DoesNotContain(histB.EnumerateArray(), x => x.GetProperty("id").GetGuid() == eA1);
        Assert.Equal(HttpStatusCode.NotFound,
            (await sstB.GetAsync($"/api/encuestas/comparacion?baseVersionId={vA1}&compararVersionId={vA2}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await sstB.GetAsync($"/api/encuestas/{eA1}/reporte?versionId={vA1}")).StatusCode);
    }

    // ---------- Apoyo ----------

    private async Task<Guid> CrearEncuestaViaApiAsync(HttpClient sst, string titulo, Guid categoriaId)
    {
        var r = await sst.PostAsJsonAsync("/api/encuestas", new
        {
            titulo,
            instrucciones = "Responde con honestidad.",
            categoriaRiesgoIds = new[] { categoriaId }
        });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }

    private async Task<Guid> PublicarPrimeraVersionAsync(HttpClient sst, Guid encuestaId)
    {
        var detalle = await Escenario.Json(await sst.GetAsync($"/api/encuestas/{encuestaId}"));
        var versionId = detalle.GetProperty("versionBorrador").GetProperty("id").GetGuid();
        var pub = await sst.PostAsync($"/api/encuestas/{encuestaId}/versiones/{versionId}/publicacion", null);
        Assert.Equal(HttpStatusCode.OK, pub.StatusCode);
        return versionId;
    }

    private async Task<Guid> PublicarNuevaVersionViaApiAsync(
        HttpClient sst,
        Guid encuestaId,
        string nuevoTextoPregunta,
        Guid peligroId,
        bool incluirSegundaPreguntaConPeligro,
        Guid? segundoPeligroId = null)
    {
        var detalle = await Escenario.Json(await sst.GetAsync($"/api/encuestas/{encuestaId}"));
        var pub = detalle.GetProperty("versionPublicada");
        var sec = pub.GetProperty("secciones")[0];
        var nuevaSeccionId = Guid.CreateVersion7();
        var preg = pub.GetProperty("preguntas")[0];

        var preguntas = new List<object>
        {
            new
            {
                id = preg.GetProperty("id").GetGuid(),
                seccionId = nuevaSeccionId,
                categoriaRiesgoId = preg.GetProperty("categoriaRiesgoId").GetGuid(),
                texto = nuevoTextoPregunta,
                tipoRespuestaCodigo = "seleccion_unica",
                obligatoria = true,
                peligroId,
                orden = (short)1,
                opciones = new[]
                {
                    new { id = (Guid?)null, texto = "Sí", orden = (short)1, indicaExposicion = true },
                    new { id = (Guid?)null, texto = "No", orden = (short)2, indicaExposicion = false }
                }
            }
        };

        if (incluirSegundaPreguntaConPeligro && segundoPeligroId.HasValue)
        {
            preguntas.Add(new
            {
                id = (Guid?)null,
                seccionId = nuevaSeccionId,
                categoriaRiesgoId = preg.GetProperty("categoriaRiesgoId").GetGuid(),
                texto = "Pregunta agregada en v2",
                tipoRespuestaCodigo = "seleccion_unica",
                obligatoria = true,
                peligroId = segundoPeligroId.Value,
                orden = (short)2,
                opciones = new[]
                {
                    new { id = (Guid?)null, texto = "Sí", orden = (short)1, indicaExposicion = true },
                    new { id = (Guid?)null, texto = "No", orden = (short)2, indicaExposicion = false }
                }
            });
        }

        var put = await sst.PutAsJsonAsync($"/api/encuestas/{encuestaId}/borrador", new
        {
            titulo = detalle.GetProperty("titulo").GetString(),
            instrucciones = "Versión actualizada",
            secciones = new[] { new { id = nuevaSeccionId, titulo = sec.GetProperty("titulo").GetString(), orden = (short)1 } },
            preguntas
        });
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        var v2Id = (await Escenario.Json(put)).GetProperty("id").GetGuid();

        var rPub = await sst.PostAsync($"/api/encuestas/{encuestaId}/versiones/{v2Id}/publicacion", null);
        Assert.Equal(HttpStatusCode.OK, rPub.StatusCode);
        return v2Id;
    }

    private async Task ResponderViaDbAsync(Guid empresaId, Guid versionId, int cantidad, bool elegirExposicion)
    {
        await using var db = api.NuevoContexto(empresaId);
        var preguntas = await db.EncuestaPregunta
            .Where(p => p.EncuestaVersionId == versionId)
            .Include(p => p.Opciones)
            .Include(p => p.TipoRespuestaPregunta)
            .OrderBy(p => p.Orden)
            .ToListAsync();

        for (var i = 0; i < cantidad; i++)
        {
            var resp = new RespuestaEncuesta
            {
                EncuestaVersionId = versionId,
                FechaRespuesta = DateOnly.FromDateTime(DateTime.UtcNow)
            };
            db.RespuestaEncuesta.Add(resp);

            foreach (var p in preguntas.Where(x => x.TipoRespuestaPregunta.Codigo == TipoRespuestaPreguntaCatalogo.CodigoSeleccionUnica))
            {
                var opcion = p.Opciones.First(o => o.IndicaExposicion == elegirExposicion);
                db.DetalleRespuesta.Add(new DetalleRespuesta
                {
                    RespuestaEncuestaId = resp.Id,
                    EncuestaPreguntaId = p.Id,
                    EncuestaOpcionId = opcion.Id
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private async Task<string> ObtenerNombreCategoriaAsync(Guid categoriaId)
    {
        await using var db = api.NuevoContexto(empresaId: null);
        return await db.CategoriaRiesgo.Where(c => c.Id == categoriaId).Select(c => c.Nombre).SingleAsync();
    }

    private async Task<short> CrearRubroAsync()
    {
        await using var db = api.NuevoContexto(empresaId: null);
        var id = (short)((await db.Rubro.MaxAsync(r => (short?)r.Id) ?? 0) + 1);
        db.Rubro.Add(new Rubro
        {
            Id = id,
            Codigo = $"rubro_{Guid.NewGuid():N}"[..30],
            Nombre = $"Rubro {Guid.NewGuid():N}"[..30]
        });
        await db.SaveChangesAsync();
        return id;
    }

    private async Task<(Guid CatErgo, Guid CatQuim, Guid PelX, Guid PelY, Guid PelZ)> CrearCatalogoAsync(short rubroId)
    {
        await using var db = api.NuevoContexto(empresaId: null);
        var tipos = await AsegurarTiposRespuestaAsync(db);

        var pelX = NuevoPeligro("Carga");
        var pelY = NuevoPeligro("Químico");
        var pelZ = NuevoPeligro("Ruido");
        var catErgo = new CategoriaRiesgo { RubroId = rubroId, Nombre = $"Ergonomía {Guid.NewGuid():N}"[..24] };
        var catQuim = new CategoriaRiesgo { RubroId = rubroId, Nombre = $"Químicos {Guid.NewGuid():N}"[..24] };

        var pErgo = new PreguntaExploratoria
        {
            CategoriaRiesgo = catErgo,
            Texto = "¿Levantas cargas pesadas?",
            TipoRespuestaPreguntaId = tipos[TipoRespuestaPreguntaCatalogo.CodigoSeleccionUnica],
            Peligro = pelX,
            Orden = 1
        };
        pErgo.Opciones.Add(new OpcionPregunta { Texto = "Sí", Orden = 1, IndicaExposicion = true });
        pErgo.Opciones.Add(new OpcionPregunta { Texto = "No", Orden = 2, IndicaExposicion = false });

        var pQuim = new PreguntaExploratoria
        {
            CategoriaRiesgo = catQuim,
            Texto = "¿Manipulas solventes?",
            TipoRespuestaPreguntaId = tipos[TipoRespuestaPreguntaCatalogo.CodigoSeleccionUnica],
            Peligro = pelZ,
            Orden = 1
        };
        pQuim.Opciones.Add(new OpcionPregunta { Texto = "Sí", Orden = 1, IndicaExposicion = true });
        pQuim.Opciones.Add(new OpcionPregunta { Texto = "No", Orden = 2, IndicaExposicion = false });

        db.AddRange(pelX, pelY, pelZ, catErgo, catQuim, pErgo, pQuim);
        await db.SaveChangesAsync();
        return (catErgo.Id, catQuim.Id, pelX.Id, pelY.Id, pelZ.Id);
    }

    private static Peligro NuevoPeligro(string prefijo) => new()
    {
        Codigo = $"{prefijo[..2].ToLowerInvariant()}_{Guid.NewGuid():N}"[..20],
        Nombre = $"{prefijo} {Guid.NewGuid():N}"[..24],
        Clasificacion = ClasificacionPeligro.Ergonomico
    };

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
}
