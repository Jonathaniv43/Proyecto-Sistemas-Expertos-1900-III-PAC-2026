using System.Net;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Cors;

/// <summary>
/// El frontend web (otro origen) puede llamar a la API desde el navegador: la preflight OPTIONS se
/// responde antes de la autenticación y los permisos, y solo para los orígenes configurados.
/// </summary>
[Collection(ColeccionApi.Nombre)]
public sealed class CorsTests(NorviaApiFactory api)
{
    private static HttpRequestMessage Preflight(string ruta, string origen, string metodo, string encabezados) => new(HttpMethod.Options, ruta)
    {
        Headers =
        {
            { "Origin", origen },
            { "Access-Control-Request-Method", metodo },
            { "Access-Control-Request-Headers", encabezados }
        }
    };

    [Theory]
    [InlineData("/api/auth/login/empresa", "POST", "content-type")]
    [InlineData("/api/encuestas", "GET", "authorization")]
    public async Task Preflight_desde_el_frontend_se_permite_aunque_la_ruta_requiera_sesion(string ruta, string metodo, string encabezados)
    {
        var respuesta = await api.CreateClient().SendAsync(Preflight(ruta, NorviaApiFactory.OrigenFrontend, metodo, encabezados));

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.Equal(NorviaApiFactory.OrigenFrontend, Assert.Single(respuesta.Headers.GetValues("Access-Control-Allow-Origin")));
    }

    [Fact]
    public async Task Preflight_desde_un_origen_no_configurado_no_recibe_permiso()
    {
        var respuesta = await api.CreateClient().SendAsync(
            Preflight("/api/auth/login/empresa", "https://sitio-ajeno.example", "POST", "content-type"));

        Assert.False(respuesta.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Un_401_lleva_los_encabezados_CORS_para_que_el_frontend_pueda_leerlo()
    {
        var peticion = new HttpRequestMessage(HttpMethod.Get, "/api/encuestas") { Headers = { { "Origin", NorviaApiFactory.OrigenFrontend } } };

        var respuesta = await api.CreateClient().SendAsync(peticion);

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        Assert.Equal(NorviaApiFactory.OrigenFrontend, Assert.Single(respuesta.Headers.GetValues("Access-Control-Allow-Origin")));
    }
}
