using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Norvia.Application.Common.Interfaces;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Empresas;

/// <summary>
/// HU-024: consulta de empresas registradas para el staff de plataforma (nombre, rubro, fecha de registro
/// y cantidad de vínculos vigentes, sin datos de SST, RN-05).
/// </summary>
[Collection(ColeccionApi.Nombre)]
public sealed class ListarEmpresasPlataformaTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Staff_consulta_empresas_con_rubro_estado_y_solo_vinculos_activos_sin_datos_sst()
    {
        var primera = await _escenario.CrearEmpresaAsync($"PlatUno{Guid.NewGuid():N}"[..14], rubroId: 1);
        var segunda = await _escenario.CrearEmpresaAsync($"PlatDos{Guid.NewGuid():N}"[..14], rubroId: null);

        // En la primera empresa: 2 activos (admin + colaborador1), 1 suspendido y 1 de baja → debe contar 2.
        await Escenario.AgregarColaboradorAsync(primera, $"act.{Guid.NewGuid():N}@pruebas.hn");
        var suspendido = await Escenario.AgregarColaboradorAsync(primera, $"sus.{Guid.NewGuid():N}@pruebas.hn");
        var baja = await Escenario.AgregarColaboradorAsync(primera, $"baj.{Guid.NewGuid():N}@pruebas.hn");
        Assert.Equal(HttpStatusCode.OK, (await primera.Cliente.PatchAsJsonAsync(
            $"/api/vinculos/{suspendido}", new { estado = "suspendido", motivoCodigo = "decision_admin" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await primera.Cliente.PatchAsJsonAsync(
            $"/api/vinculos/{baja}", new { estado = "baja", motivoCodigo = "baja_laboral" })).StatusCode);

        await using (var db = api.NuevoContexto(empresaId: null))
        {
            var e2 = await db.Empresa.SingleAsync(e => e.Id == segunda.EmpresaId);
            e2.Estado = EstadoEmpresa.Suspendida;
            await db.SaveChangesAsync();
        }

        var adminPlataforma = await _escenario.ClienteStaffAsync();
        var soportePlataforma = await ClienteSoportePlataformaAsync();

        foreach (var cliente in new[] { adminPlataforma, soportePlataforma })
        {
            var r = await cliente.GetAsync("/api/plataforma/empresas");
            Assert.Equal(HttpStatusCode.OK, r.StatusCode);

            var lista = (await Escenario.Json(r)).EnumerateArray().ToList();
            var item1 = lista.Single(x => x.GetProperty("empresaId").GetGuid() == primera.EmpresaId);
            var item2 = lista.Single(x => x.GetProperty("empresaId").GetGuid() == segunda.EmpresaId);

            // Orden descendente por fecha de registro: la segunda creada va antes que la primera.
            Assert.True(lista.IndexOf(item2) < lista.IndexOf(item1));

            Assert.Equal("activa", item1.GetProperty("estado").GetString());
            Assert.False(string.IsNullOrWhiteSpace(item1.GetProperty("rubroNombre").GetString()));
            Assert.NotEqual(default, item1.GetProperty("registradaEn").GetDateTime());
            Assert.Equal(2, item1.GetProperty("vinculosActivos").GetInt32());

            Assert.Equal("suspendida", item2.GetProperty("estado").GetString());
            Assert.Equal(System.Text.Json.JsonValueKind.Null, item2.GetProperty("rubroNombre").ValueKind);
            Assert.Equal(1, item2.GetProperty("vinculosActivos").GetInt32());

            // No expone datos de SST ni listas de personas/correos (RN-05).
            Assert.False(item1.TryGetProperty("rtn", out _));
            Assert.False(item1.TryGetProperty("encuestas", out _));
            Assert.False(item1.TryGetProperty("riesgos", out _));
            Assert.False(item1.TryGetProperty("accidentes", out _));
            Assert.False(item1.TryGetProperty("personas", out _));
        }
    }

    [Fact]
    public async Task Buscar_filtra_por_razon_social_o_rubro_y_rechaza_filtro_largo()
    {
        var marca = $"Unica{Guid.NewGuid():N}"[..14];
        var conRubro = await _escenario.CrearEmpresaAsync(marca, rubroId: 1);
        var sinRubro = await _escenario.CrearEmpresaAsync($"Otro{Guid.NewGuid():N}"[..14], rubroId: null);
        var staff = await _escenario.ClienteStaffAsync();

        string rubroNombre;
        await using (var db = api.NuevoContexto(empresaId: null))
            rubroNombre = await db.Rubro.Where(r => r.Id == 1).Select(r => r.Nombre).SingleAsync();

        var porNombre = (await Escenario.Json(await staff.GetAsync($"/api/plataforma/empresas?buscar={marca.ToLowerInvariant()}")))
            .EnumerateArray().ToList();
        Assert.Single(porNombre);
        Assert.Equal(conRubro.EmpresaId, porNombre[0].GetProperty("empresaId").GetGuid());

        var porRubro = (await Escenario.Json(await staff.GetAsync($"/api/plataforma/empresas?buscar={Uri.EscapeDataString(rubroNombre)}")))
            .EnumerateArray().Select(x => x.GetProperty("empresaId").GetGuid()).ToList();
        Assert.Contains(conRubro.EmpresaId, porRubro);
        Assert.DoesNotContain(sinRubro.EmpresaId, porRubro);

        var largo = await staff.GetAsync($"/api/plataforma/empresas?buscar={new string('a', 101)}");
        Assert.Equal(HttpStatusCode.BadRequest, largo.StatusCode);
    }

    [Fact]
    public async Task Usuarios_de_empresa_y_anonimos_no_pueden_consultar_empresas_de_plataforma()
    {
        var empresa = await _escenario.CrearEmpresaAsync("PlatPerm");
        var sst = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");
        var encuestado = await _escenario.ClienteConRolAsync(empresa, "encuestado");

        Assert.Equal(HttpStatusCode.Unauthorized, (await api.CreateClient().GetAsync("/api/plataforma/empresas")).StatusCode);
        foreach (var cliente in new[] { empresa.Cliente, sst, colaborador, encuestado })
            Assert.Equal(HttpStatusCode.Forbidden, (await cliente.GetAsync("/api/plataforma/empresas")).StatusCode);
    }

    private async Task<HttpClient> ClienteSoportePlataformaAsync()
    {
        using var scope = api.Services.CreateScope();
        var hasher = scope.ServiceProvider.GetRequiredService<IHasherPassword>();
        var email = $"soporte.{Guid.NewGuid():N}@norvia.hn";
        const string password = "Soporte#Pruebas2026";

        await using (var db = api.NuevoContexto(empresaId: null))
        {
            var rolSoporteId = await db.RolStaffPlataforma
                .Where(r => r.Codigo == "soporte_plataforma")
                .Select(r => r.Id)
                .SingleAsync();
            var persona = new Persona { PrimerNombre = "Soporte", PrimerApellido = "Plataforma" };
            var acceso = new Acceso { Email = email, Verificado = true };
            acceso.PasswordHash = hasher.Hashear(acceso, password);
            db.AddRange(
                persona,
                new Usuario { PersonaId = persona.Id },
                acceso,
                new StaffPlataforma { AccesoId = acceso.Id, RolStaffPlataformaId = rolSoporteId });
            await db.SaveChangesAsync();
        }

        var login = await api.CreateClient().PostAsJsonAsync("/api/auth/login/plataforma", new { email, password });
        login.EnsureSuccessStatusCode();
        var token = (await Escenario.Json(login)).GetProperty("accessToken").GetString();
        var cliente = api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }
}
