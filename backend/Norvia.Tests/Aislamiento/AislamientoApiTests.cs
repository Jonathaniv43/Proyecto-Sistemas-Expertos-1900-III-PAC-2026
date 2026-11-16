using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Aislamiento;

/// <summary>
/// HU-098 por la API: una persona autorizada en la empresa A no puede consultar, registrar, editar ni
/// retirar información de la empresa B (RN-05, RN-06). Cada prueba crea sus propias empresas.
/// </summary>
[Collection(ColeccionApi.Nombre)]
public sealed class AislamientoApiTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    // ---------- Consultar ----------

    [Fact]
    public async Task Listar_vinculos_solo_devuelve_los_de_la_empresa_de_la_sesion()
    {
        var a = await _escenario.CrearEmpresaAsync("A");
        var b = await _escenario.CrearEmpresaAsync("B");
        var colaboradorB = await Escenario.AgregarColaboradorAsync(b, $"colab.{Guid.NewGuid():N}@pruebas.hn");

        var lista = await Escenario.Json(await a.Cliente.GetAsync("/api/vinculos"));

        var ids = lista.EnumerateArray().Select(v => v.GetProperty("id").GetGuid()).ToList();
        Assert.Equal([a.AdminVinculoId], ids);
        Assert.DoesNotContain(colaboradorB, ids);
        Assert.DoesNotContain(b.AdminVinculoId, ids);
    }

    [Fact]
    public async Task Ver_un_vinculo_de_otra_empresa_responde_404()
    {
        var a = await _escenario.CrearEmpresaAsync("A");
        var b = await _escenario.CrearEmpresaAsync("B");

        var r = await a.Cliente.GetAsync($"/api/vinculos/{b.AdminVinculoId}");

        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }

    // ---------- Registrar ----------

    [Fact]
    public async Task El_alta_usa_la_empresa_del_token_aunque_el_cliente_envie_otra()
    {
        var a = await _escenario.CrearEmpresaAsync("A");
        var b = await _escenario.CrearEmpresaAsync("B");

        // Un campo empresaId en el cuerpo no existe en el contrato: se ignora (RN-06).
        var r = await a.Cliente.PostAsJsonAsync("/api/vinculos", new
        {
            email = $"intruso.{Guid.NewGuid():N}@pruebas.hn", primerNombre = "X", primerApellido = "Y",
            rolCodigo = "colaborador", passwordTemporal = "Temporal#2026", empresaId = b.EmpresaId
        });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var id = (await Escenario.Json(r)).GetProperty("id").GetGuid();

        await using var db = api.NuevoContexto(empresaId: null);
        var empresaDelVinculo = await db.VinculoEmpresa.IgnoreQueryFilters()
            .Where(v => v.Id == id).Select(v => v.EmpresaId).SingleAsync();
        Assert.Equal(a.EmpresaId, empresaDelVinculo);
    }

    [Fact]
    public async Task Dar_acceso_en_A_a_una_persona_de_B_no_cambia_nada_en_B()
    {
        var a = await _escenario.CrearEmpresaAsync("A");
        var b = await _escenario.CrearEmpresaAsync("B");
        var antes = await Escenario.Json(await b.Cliente.GetAsync("/api/vinculos"));

        var r = await a.Cliente.PostAsJsonAsync("/api/vinculos", new
        {
            email = b.AdminEmail, primerNombre = "Otro", primerApellido = "Nombre", rolCodigo = "colaborador"
        });

        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var despues = await Escenario.Json(await b.Cliente.GetAsync("/api/vinculos"));
        Assert.Equal(antes.ToString(), despues.ToString());
    }

    // ---------- Editar ----------

    [Fact]
    public async Task Editar_un_vinculo_de_otra_empresa_responde_404_y_no_lo_cambia()
    {
        var a = await _escenario.CrearEmpresaAsync("A");
        var b = await _escenario.CrearEmpresaAsync("B");
        var colaboradorB = await Escenario.AgregarColaboradorAsync(b, $"colab.{Guid.NewGuid():N}@pruebas.hn");

        var r = await a.Cliente.PatchAsJsonAsync($"/api/vinculos/{colaboradorB}", new { rolCodigo = "admin_empresa" });

        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
        var enB = await Escenario.Json(await b.Cliente.GetAsync($"/api/vinculos/{colaboradorB}"));
        Assert.Equal("colaborador", enB.GetProperty("rolCodigo").GetString());
    }

    // ---------- Retirar ----------

    [Fact]
    public async Task Suspender_o_dar_de_baja_un_vinculo_de_otra_empresa_responde_404()
    {
        var a = await _escenario.CrearEmpresaAsync("A");
        var b = await _escenario.CrearEmpresaAsync("B");

        var r = await a.Cliente.PatchAsJsonAsync($"/api/vinculos/{b.AdminVinculoId}",
            new { estado = "baja", motivoCodigo = "decision_admin" });

        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
        var enB = await Escenario.Json(await b.Cliente.GetAsync($"/api/vinculos/{b.AdminVinculoId}"));
        Assert.Equal("activo", enB.GetProperty("estado").GetString());
    }

    [Fact]
    public async Task Suspender_a_una_persona_en_A_no_afecta_su_acceso_a_B()
    {
        var a = await _escenario.CrearEmpresaAsync("A");
        var b = await _escenario.CrearEmpresaAsync("B");
        var email = $"compartido.{Guid.NewGuid():N}@pruebas.hn";
        var enA = await Escenario.AgregarColaboradorAsync(a, email);
        await Escenario.AgregarColaboradorAsync(b, email); // misma cuenta, otra empresa (RN-02)

        var r = await a.Cliente.PatchAsJsonAsync($"/api/vinculos/{enA}",
            new { estado = "suspendido", motivoCodigo = "decision_admin" });
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await _escenario.LoginEmpresaAsync(email, "Temporal#2026", a.IdEmpresaCifrado)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _escenario.LoginEmpresaAsync(email, "Temporal#2026", b.IdEmpresaCifrado)).StatusCode);
    }

    // ---------- Sesión ----------

    [Fact]
    public async Task No_se_puede_iniciar_sesion_en_una_empresa_donde_no_se_tiene_vinculo()
    {
        var a = await _escenario.CrearEmpresaAsync("A");
        var b = await _escenario.CrearEmpresaAsync("B");

        var r = await _escenario.LoginEmpresaAsync(a.AdminEmail, Escenario.PasswordAdmin, b.IdEmpresaCifrado);

        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task Un_identificador_de_empresa_alterado_no_abre_sesion()
    {
        var a = await _escenario.CrearEmpresaAsync("A");
        var alterado = a.IdEmpresaCifrado[..^4] + (a.IdEmpresaCifrado[^4] == 'A' ? "B" : "A") + a.IdEmpresaCifrado[^3..];

        var r = await _escenario.LoginEmpresaAsync(a.AdminEmail, Escenario.PasswordAdmin, alterado);

        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task El_staff_de_plataforma_no_accede_a_datos_de_las_empresas()
    {
        await _escenario.CrearEmpresaAsync("A");
        var staff = await _escenario.ClienteStaffAsync();

        Assert.Equal(HttpStatusCode.Forbidden, (await staff.GetAsync("/api/vinculos")).StatusCode);
    }

    [Fact]
    public async Task Una_empresa_no_puede_ver_editar_publicar_ni_cerrar_encuestas_de_otra()
    {
        short rubroId;
        Guid categoriaId;
        await using (var db = api.NuevoContexto(empresaId: null))
        {
            var tipoBreve = await db.TipoRespuestaPregunta
                .Where(t => t.Codigo == TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve)
                .Select(t => (Guid?)t.Id)
                .FirstOrDefaultAsync();
            if (tipoBreve is null)
            {
                var fila = new TipoRespuestaPreguntaCatalogo
                {
                    Id = Guid.CreateVersion7(),
                    Codigo = TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve,
                    Nombre = "Respuesta breve",
                    InstruccionUi = TipoRespuestaPreguntaCatalogo.InstruccionUiPorDefecto(TipoRespuestaPreguntaCatalogo.CodigoRespuestaBreve)
                };
                db.TipoRespuestaPregunta.Add(fila);
                await db.SaveChangesAsync();
                tipoBreve = fila.Id;
            }

            rubroId = (short)(await db.Rubro.MaxAsync(r => r.Id) + 1);
            var rubro = new Rubro { Id = rubroId, Codigo = $"r_{Guid.NewGuid():N}"[..20], Nombre = $"Rubro {Guid.NewGuid():N}" };
            var cat = new CategoriaRiesgo { Rubro = rubro, Nombre = $"Cat {Guid.NewGuid():N}"[..20] };
            db.AddRange(rubro, cat,
                new PreguntaExploratoria { CategoriaRiesgo = cat, Texto = "¿Hay ruido?", TipoRespuestaPreguntaId = tipoBreve.Value, Orden = 1 });
            await db.SaveChangesAsync();
            categoriaId = cat.Id;
        }

        var a = await _escenario.CrearEmpresaAsync("EncIsoA", rubroId);
        var b = await _escenario.CrearEmpresaAsync("EncIsoB", rubroId);
        var sstA = await _escenario.ClienteConRolAsync(a, "responsable_sst");
        var sstB = await _escenario.ClienteConRolAsync(b, "responsable_sst");

        var creada = await Escenario.Json(await sstA.PostAsJsonAsync("/api/encuestas", new
        {
            titulo = "Encuesta privada A",
            categoriaRiesgoIds = new[] { categoriaId }
        }));
        var encuestaAId = creada.GetProperty("id").GetGuid();
        var versionAId = creada.GetProperty("versionBorradorId").GetGuid();

        Assert.Empty((await Escenario.Json(await sstB.GetAsync("/api/encuestas"))).EnumerateArray());
        Assert.Equal(HttpStatusCode.NotFound, (await sstB.GetAsync($"/api/encuestas/{encuestaAId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await sstB.PutAsJsonAsync($"/api/encuestas/{encuestaAId}/borrador", new
        {
            secciones = Array.Empty<object>(),
            preguntas = new object[]
            {
                new { texto = "Intento", tipoRespuestaCodigo = "respuesta_breve", obligatoria = true, orden = 1, opciones = Array.Empty<object>() }
            }
        })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await sstB.PostAsync($"/api/encuestas/{encuestaAId}/versiones/{versionAId}/publicacion", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await sstB.PostAsync($"/api/encuestas/{encuestaAId}/cierre", null)).StatusCode);
    }
}
