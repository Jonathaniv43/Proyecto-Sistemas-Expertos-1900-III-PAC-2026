using System.Net;
using Microsoft.EntityFrameworkCore;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Empresas;

/// <summary>HU-017: el formulario de registro obtiene los rubros sin sesión.</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class RubrosRegistroTests(NorviaApiFactory api)
{
    [Fact]
    public async Task Sin_sesion_devuelve_los_rubros_activos_con_codigo_y_nombre()
    {
        var r = await api.CreateClient().GetAsync("/api/empresas/rubros-registro");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var rubros = (await Escenario.Json(r)).EnumerateArray().ToList();
        Assert.NotEmpty(rubros);
        Assert.Contains(rubros, x => x.GetProperty("codigo").GetString() == "oficina");
        // Solo lo necesario para elegir: sin Id ni estado.
        Assert.All(rubros, x => Assert.Equal(["codigo", "nombre"], x.EnumerateObject().Select(p => p.Name).Order()));
    }

    [Fact]
    public async Task No_incluye_rubros_desactivados()
    {
        await using (var db = api.NuevoContexto(empresaId: null))
            await db.Rubro.Where(x => x.Codigo == "comercio").ExecuteUpdateAsync(s => s.SetProperty(x => x.Activo, false));
        try
        {
            var r = await api.CreateClient().GetAsync("/api/empresas/rubros-registro");

            var codigos = (await Escenario.Json(r)).EnumerateArray().Select(x => x.GetProperty("codigo").GetString()).ToList();
            Assert.DoesNotContain("comercio", codigos);
            Assert.Contains("oficina", codigos);
        }
        finally
        {
            await using var db = api.NuevoContexto(empresaId: null);
            await db.Rubro.Where(x => x.Codigo == "comercio").ExecuteUpdateAsync(s => s.SetProperty(x => x.Activo, true));
        }
    }
}
