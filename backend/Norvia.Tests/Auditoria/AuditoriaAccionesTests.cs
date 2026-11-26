using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Auditoria;

/// <summary>
/// HU-099: registro automático, cronológico, filtrado, aislado e inalterable de auditoría (RN-05, RN-72, D11).
/// </summary>
[Collection(ColeccionApi.Nombre)]
public sealed class AuditoriaAccionesTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-6));

    [Fact]
    public async Task Registra_acciones_clave_de_gestion_y_conserva_quien_que_cuando_y_detalle()
    {
        var rubro = await CrearRubroAsync();
        var (categoriaId, peligroId) = await CrearCatalogoAsync(rubro);
        var plantillaId = await CrearPlantillaDocumentoAsync();

        var empresa = await _escenario.CrearEmpresaAsync("AudFull", rubro);
        var (sst, sstVinculoId) = await CrearVinculoYClienteAsync(empresa, "responsable_sst", $"sst.{Guid.NewGuid():N}@pruebas.hn");
        var colabId = await Escenario.AgregarColaboradorAsync(empresa, $"col.{Guid.NewGuid():N}@pruebas.hn");

        // M1: perfil empresa + actualizar vínculo
        Assert.Equal(HttpStatusCode.OK, (await empresa.Cliente.PatchAsJsonAsync("/api/empresas/perfil", new
        {
            trabajadoresPermanentes = 25,
            direccion = "Blvd. Morazán"
        })).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await empresa.Cliente.PatchAsJsonAsync($"/api/vinculos/{colabId}", new
        {
            estado = "suspendido",
            motivoCodigo = "decision_admin"
        })).StatusCode);

        // M3: crear, publicar y cerrar encuesta
        var crearEnc = await sst.PostAsJsonAsync("/api/encuestas", new
        {
            titulo = "Diagnóstico Auditoría 2026",
            categoriaRiesgoIds = new[] { categoriaId }
        });
        Assert.Equal(HttpStatusCode.Created, crearEnc.StatusCode);
        var encJson = await Escenario.Json(crearEnc);
        var encuestaId = encJson.GetProperty("id").GetGuid();
        var versionEncId = encJson.GetProperty("versionBorradorId").GetGuid();

        Assert.Equal(HttpStatusCode.OK,
            (await sst.PostAsync($"/api/encuestas/{encuestaId}/versiones/{versionEncId}/publicacion", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await sst.PostAsync($"/api/encuestas/{encuestaId}/cierre", null)).StatusCode);

        // M4: crear riesgo, evaluar, agregar control
        var crearRiesgo = await sst.PostAsJsonAsync("/api/riesgos", new
        {
            peligroId,
            actividad = "Área de empaque"
        });
        Assert.Equal(HttpStatusCode.Created, crearRiesgo.StatusCode);
        var riesgoId = (await Escenario.Json(crearRiesgo)).GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.Created,
            (await sst.PostAsJsonAsync($"/api/riesgos/{riesgoId}/evaluaciones", new { probabilidad = 3, severidad = 4 })).StatusCode);

        Assert.Equal(HttpStatusCode.Created,
            (await sst.PostAsJsonAsync($"/api/riesgos/{riesgoId}/controles", new
            {
                tipo = "administrativo",
                descripcion = "Señalización preventiva"
            })).StatusCode);

        // M6 + M5: evento -> investigar -> medida -> estado tarea -> cierre evento
        var crearEv = await sst.PostAsJsonAsync("/api/eventos", new
        {
            tipo = "accidente",
            esGrave = false,
            fechaHoraEvento = $"{Hoy:yyyy-MM-dd}T08:30:00-06:00",
            descripcion = "Tropiezo en pasillo"
        });
        Assert.Equal(HttpStatusCode.Created, crearEv.StatusCode);
        var eventoId = (await Escenario.Json(crearEv)).GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.OK, (await sst.PutAsJsonAsync($"/api/eventos/{eventoId}/investigacion", new
        {
            causasInmediatas = "Piso húmedo",
            causasBasicas = "Sin señalización",
            investigadaPorId = sstVinculoId,
            fechaInvestigacion = Hoy.ToString("yyyy-MM-dd")
        })).StatusCode);

        var crearMedida = await sst.PostAsJsonAsync($"/api/eventos/{eventoId}/medidas-correctivas", new
        {
            titulo = "Colocar cinta antideslizante",
            prioridad = 1,
            responsableId = sstVinculoId,
            fechaLimite = Hoy.AddDays(5).ToString("yyyy-MM-dd"),
            requiereEvidencia = false
        });
        Assert.Equal(HttpStatusCode.Created, crearMedida.StatusCode);
        var tareaId = (await Escenario.Json(crearMedida)).GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.OK,
            (await sst.PatchAsJsonAsync($"/api/plan-trabajo/tareas/{tareaId}/estado", new { estado = "en_progreso" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await sst.PostAsync($"/api/eventos/{eventoId}/cierre", null)).StatusCode);

        // M9: generar y aprobar documento
        var genDoc = await sst.PostAsJsonAsync("/api/documentos/generacion", new
        {
            plantillaDocumentoId = plantillaId,
            titulo = "Política SST Auditada"
        });
        Assert.Equal(HttpStatusCode.Created, genDoc.StatusCode);
        var docId = (await Escenario.Json(genDoc)).GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.OK,
            (await sst.PostAsync($"/api/documentos/{docId}/aprobacion", null)).StatusCode);

        // Consultar bitácora completa
        var resp = await empresa.Cliente.GetAsync("/api/auditoria?tamano=50");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var bitacora = await Escenario.Json(resp);
        var items = bitacora.GetProperty("items").EnumerateArray().ToList();

        Assert.True(bitacora.GetProperty("total").GetInt32() >= 14);
        var acciones = items.Select(i => i.GetProperty("accion").GetString()).ToList();

        foreach (var esperada in new[]
        {
            "vinculo_creado",
            "empresa_perfil_actualizado",
            "vinculo_actualizado",
            "encuesta_creada",
            "encuesta_publicada",
            "encuesta_cerrada",
            "riesgo_creado",
            "riesgo_evaluado",
            "control_creado",
            "evento_investigado",
            "medida_correctiva_creada",
            "tarea_estado_cambiado",
            "evento_cerrado",
            "documento_generado",
            "documento_aprobado"
        })
        {
            Assert.Contains(esperada, acciones);
        }

        // Orden cronológico descendente y metadatos de actor en todos los registros
        for (var i = 0; i < items.Count - 1; i++)
        {
            var actual = items[i].GetProperty("fecha").GetDateTime();
            var siguiente = items[i + 1].GetProperty("fecha").GetDateTime();
            Assert.True(actual >= siguiente);
        }

        Assert.All(items, item =>
        {
            Assert.Equal("empresa", item.GetProperty("actorTipo").GetString());
            Assert.False(string.IsNullOrWhiteSpace(item.GetProperty("actorNombre").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(item.GetProperty("actorCorreo").GetString()));
        });

        var aprobacion = items.First(i => i.GetProperty("accion").GetString() == "documento_aprobado");
        Assert.Equal("Documento", aprobacion.GetProperty("entidad").GetString());
        Assert.Equal(docId, aprobacion.GetProperty("entidadId").GetGuid());
        Assert.Equal("Política SST Auditada", aprobacion.GetProperty("detalle").GetProperty("titulo").GetString());
    }

    [Fact]
    public async Task Nunca_registra_acciones_anonimas_de_respuestas_ni_reporte_de_eventos()
    {
        var rubro = await CrearRubroAsync();
        var (categoriaId, _) = await CrearCatalogoAsync(rubro);
        var empresa = await _escenario.CrearEmpresaAsync("AudAnon", rubro);
        var (sst, _) = await CrearVinculoYClienteAsync(empresa, "responsable_sst", $"sst.{Guid.NewGuid():N}@pruebas.hn");
        var (encuestado, encuestadoVinculoId) = await CrearVinculoYClienteAsync(empresa, "encuestado", $"enc.{Guid.NewGuid():N}@pruebas.hn");

        var creada = await Escenario.Json(await sst.PostAsJsonAsync("/api/encuestas", new
        {
            titulo = "Encuesta Anónima",
            categoriaRiesgoIds = new[] { categoriaId }
        }));
        var encuestaId = creada.GetProperty("id").GetGuid();
        var versionId = creada.GetProperty("versionBorradorId").GetGuid();
        Assert.Equal(HttpStatusCode.OK,
            (await sst.PostAsync($"/api/encuestas/{encuestaId}/versiones/{versionId}/publicacion", null)).StatusCode);

        var pendientes = await Escenario.Json(await encuestado.GetAsync("/api/encuestas/pendientes"));
        var preguntaId = pendientes.EnumerateArray()
            .Single(e => e.GetProperty("encuestaId").GetGuid() == encuestaId)
            .GetProperty("preguntas")[0].GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.OK, (await encuestado.PostAsJsonAsync($"/api/encuestas/{encuestaId}/respuestas", new
        {
            versionId,
            respuestas = new[] { new { preguntaId, textoLibre = "Respuesta anónima" } }
        })).StatusCode);

        Assert.Equal(HttpStatusCode.Created, (await encuestado.PostAsJsonAsync("/api/eventos", new
        {
            tipo = "condicion_insegura",
            esGrave = false,
            fechaHoraEvento = DateTime.UtcNow.AddMinutes(-5),
            descripcion = "Barandal flojo"
        })).StatusCode);

        await using var db = api.NuevoContexto(empresa.EmpresaId);
        var registros = await db.RegistroAuditoria.AsNoTracking().ToListAsync();
        Assert.DoesNotContain(registros, r => r.VinculoEmpresaId == encuestadoVinculoId);
        Assert.DoesNotContain(registros, r => r.Entidad == "Accidente");
    }

    [Fact]
    public async Task Solo_roles_de_gestion_consultan_y_con_aislamiento_entre_empresas()
    {
        var empresaA = await _escenario.CrearEmpresaAsync("AudIsoA");
        var empresaB = await _escenario.CrearEmpresaAsync("AudIsoB");
        var sstA = await _escenario.ClienteConRolAsync(empresaA, "responsable_sst");
        var colaboradorA = await _escenario.ClienteConRolAsync(empresaA, "colaborador");
        var encuestadoA = await _escenario.ClienteConRolAsync(empresaA, "encuestado");

        Assert.Equal(HttpStatusCode.OK, (await empresaA.Cliente.GetAsync("/api/auditoria")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await sstA.GetAsync("/api/auditoria")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await colaboradorA.GetAsync("/api/auditoria")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await encuestadoA.GetAsync("/api/auditoria")).StatusCode);

        var itemsA = (await Escenario.Json(await empresaA.Cliente.GetAsync("/api/auditoria"))).GetProperty("items")
            .EnumerateArray().Select(i => i.GetProperty("id").GetGuid()).ToList();
        var itemsB = (await Escenario.Json(await empresaB.Cliente.GetAsync("/api/auditoria"))).GetProperty("items")
            .EnumerateArray().Select(i => i.GetProperty("id").GetGuid()).ToList();

        Assert.NotEmpty(itemsA);
        Assert.Empty(itemsB);
        Assert.DoesNotContain(itemsA[0], itemsB);
    }

    [Fact]
    public async Task Filtros_y_paginacion_funcionan_y_validan_rango_fechas()
    {
        var rubro = await CrearRubroAsync();
        var empresa = await _escenario.CrearEmpresaAsync("AudFiltros", rubro);
        var (sst, _) = await CrearVinculoYClienteAsync(empresa, "responsable_sst", $"sst.{Guid.NewGuid():N}@pruebas.hn");
        await Escenario.AgregarColaboradorAsync(empresa, $"buscable.{Guid.NewGuid():N}@pruebas.hn");
        var plantillaId = await CrearPlantillaDocumentoAsync();
        var gen = await sst.PostAsJsonAsync("/api/documentos/generacion", new
        {
            plantillaDocumentoId = plantillaId,
            titulo = "Política Filtro Especial"
        });
        Assert.Equal(HttpStatusCode.Created, gen.StatusCode);

        var porEntidad = await Escenario.Json(await empresa.Cliente.GetAsync("/api/auditoria?entidad=Documento"));
        var itemsDoc = porEntidad.GetProperty("items").EnumerateArray().ToList();
        Assert.Single(itemsDoc);
        Assert.Equal("Documento", itemsDoc[0].GetProperty("entidad").GetString());

        var porTexto = await Escenario.Json(await empresa.Cliente.GetAsync("/api/auditoria?buscar=Filtro%20Especial"));
        Assert.Equal(1, porTexto.GetProperty("total").GetInt32());

        var sinCoincidencia = await Escenario.Json(await empresa.Cliente.GetAsync("/api/auditoria?buscar=inexistente_xyz_999"));
        Assert.Equal(0, sinCoincidencia.GetProperty("total").GetInt32());
        Assert.Equal(1, sinCoincidencia.GetProperty("totalPaginas").GetInt32());

        var porFecha = await Escenario.Json(await empresa.Cliente.GetAsync($"/api/auditoria?desde={Hoy:yyyy-MM-dd}&hasta={Hoy:yyyy-MM-dd}"));
        Assert.True(porFecha.GetProperty("total").GetInt32() >= 3);

        var rangoInvalido = await empresa.Cliente.GetAsync($"/api/auditoria?desde={Hoy.AddDays(1):yyyy-MM-dd}&hasta={Hoy:yyyy-MM-dd}");
        Assert.Equal(HttpStatusCode.BadRequest, rangoInvalido.StatusCode);

        var p1 = await Escenario.Json(await empresa.Cliente.GetAsync("/api/auditoria?pagina=1&tamano=2"));
        var p2 = await Escenario.Json(await empresa.Cliente.GetAsync("/api/auditoria?pagina=2&tamano=2"));
        var ids1 = p1.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetGuid()).ToList();
        var ids2 = p2.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetGuid()).ToList();

        Assert.Equal(2, ids1.Count);
        Assert.NotEmpty(ids2);
        Assert.Empty(ids1.Intersect(ids2));
        Assert.True(p1.GetProperty("totalPaginas").GetInt32() >= 2);
    }

    [Fact]
    public async Task Endpoint_solo_lectura_no_admite_mutaciones_http()
    {
        var empresa = await _escenario.CrearEmpresaAsync("AudRo");
        var id = Guid.NewGuid();

        var post = await empresa.Cliente.PostAsJsonAsync("/api/auditoria", new { });
        var put = await empresa.Cliente.PutAsJsonAsync($"/api/auditoria/{id}", new { });
        var patch = await empresa.Cliente.PatchAsJsonAsync($"/api/auditoria/{id}", new { });
        var del = await empresa.Cliente.DeleteAsync($"/api/auditoria/{id}");

        Assert.Equal(HttpStatusCode.MethodNotAllowed, post.StatusCode);
        Assert.False(put.IsSuccessStatusCode);
        Assert.False(patch.IsSuccessStatusCode);
        Assert.False(del.IsSuccessStatusCode);
    }

    // ---------- Apoyo ----------

    private async Task<(HttpClient Cliente, Guid VinculoId)> CrearVinculoYClienteAsync(
        EmpresaDePrueba empresa, string rolCodigo, string email)
    {
        const string password = "Temporal#2026";
        var r = await empresa.Cliente.PostAsJsonAsync("/api/vinculos", new
        {
            email,
            primerNombre = "Persona",
            primerApellido = "Auditoria",
            rolCodigo,
            passwordTemporal = password
        });
        r.EnsureSuccessStatusCode();
        var vinculoId = (await Escenario.Json(r)).GetProperty("id").GetGuid();
        var cliente = await _escenario.ClienteEmpresaAsync(email, password, empresa.IdEmpresaCifrado);
        return (cliente, vinculoId);
    }

    private async Task<short> CrearRubroAsync()
    {
        await using var db = api.NuevoContexto(empresaId: null);
        var id = (short)(await db.Rubro.MaxAsync(r => r.Id) + 1);
        db.Rubro.Add(new Rubro { Id = id, Codigo = $"raud_{Guid.NewGuid():N}"[..25], Nombre = $"Rubro Aud {id}" });
        await db.SaveChangesAsync();
        return id;
    }

    private async Task<(Guid CategoriaId, Guid PeligroId)> CrearCatalogoAsync(short rubroId)
    {
        await using var db = api.NuevoContexto(empresaId: null);
        var tipos = await db.TipoRespuestaPregunta.ToDictionaryAsync(t => t.Codigo, t => t.Id);
        if (tipos.Count == 0)
        {
            var t = new TipoRespuestaPreguntaCatalogo
            {
                Id = Guid.CreateVersion7(),
                Codigo = TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve,
                Nombre = "Respuesta breve",
                InstruccionUi = TipoRespuestaPreguntaCatalogo.InstruccionUiPorDefecto(TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve),
                MaxCaracteresRespuesta = 500
            };
            db.TipoRespuestaPregunta.Add(t);
            tipos[t.Codigo] = t.Id;
        }

        var peligro = new Peligro
        {
            Codigo = $"paud_{Guid.NewGuid():N}"[..20],
            Nombre = "Piso resbaloso",
            Clasificacion = ClasificacionPeligro.Ergonomico
        };
        var categoria = new CategoriaRiesgo { RubroId = rubroId, Nombre = "Locativo" };
        var pregunta = new PreguntaExploratoria
        {
            CategoriaRiesgo = categoria,
            Texto = "¿Hay pasillos obstruidos?",
            TipoRespuestaPreguntaId = tipos[TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve],
            Obligatoria = false,
            Orden = 1
        };
        db.AddRange(peligro, categoria, pregunta);
        await db.SaveChangesAsync();
        return (categoria.Id, peligro.Id);
    }

    private async Task<Guid> CrearPlantillaDocumentoAsync()
    {
        var plantilla = new PlantillaDocumento
        {
            Codigo = $"pl_{Guid.NewGuid():N}"[..20],
            Titulo = "Plantilla Política Auditoría",
            TipoDocumento = TipoDocumento.Politica,
            Contenido = "# Política de {{RazonSocial}}\n\nTexto base sin faltantes."
        };
        await using var db = api.NuevoContexto(empresaId: null);
        db.PlantillaDocumento.Add(plantilla);
        await db.SaveChangesAsync();
        return plantilla.Id;
    }
}
