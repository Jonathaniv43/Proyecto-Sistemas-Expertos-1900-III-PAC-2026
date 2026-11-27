using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Norvia.Application.Common.Interfaces;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Auth;

/// <summary>Verificación de cuenta sin token (correo + código de 6 dígitos + nueva contraseña; RN-67, RN-86).</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class VerificarCuentaTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    private const string Codigo = "123456";
    private const string Nueva = "SeguraEmpresa9!";

    [Fact]
    public async Task Sin_verificar_no_hay_login_y_tras_verificar_si()
    {
        var (email, idEmpresa) = await CrearAccesoPendienteAsync("VerifOk");

        var verificar = await api.CreateClient().PostAsJsonAsync("/api/auth/cuenta/verificar", new
        {
            email,
            codigo = Codigo,
            contrasenaNueva = Nueva,
            contrasenaNuevaRepetida = Nueva
        });
        Assert.Equal(HttpStatusCode.NoContent, verificar.StatusCode);

        Assert.Equal(HttpStatusCode.OK,
            (await _escenario.LoginEmpresaAsync(email, Nueva, idEmpresa)).StatusCode);
    }

    [Fact]
    public async Task Codigo_incorrecto_o_email_desconocido_devuelve_error()
    {
        var (email, _) = await CrearAccesoPendienteAsync("VerifBad");

        var malCodigo = await api.CreateClient().PostAsJsonAsync("/api/auth/cuenta/verificar", new
        {
            email,
            codigo = "000000",
            contrasenaNueva = Nueva,
            contrasenaNuevaRepetida = Nueva
        });
        Assert.Equal(HttpStatusCode.BadRequest, malCodigo.StatusCode);

        var desconocido = await api.CreateClient().PostAsJsonAsync("/api/auth/cuenta/verificar", new
        {
            email = "no.existe@pruebas.hn",
            codigo = Codigo,
            contrasenaNueva = Nueva,
            contrasenaNuevaRepetida = Nueva
        });
        Assert.Equal(HttpStatusCode.Unauthorized, desconocido.StatusCode);
    }

    [Fact]
    public async Task Cuenta_ya_verificada_devuelve_conflicto()
    {
        var (email, _) = await CrearAccesoPendienteAsync("VerifDup");

        var cliente = api.CreateClient();
        var ok = await cliente.PostAsJsonAsync("/api/auth/cuenta/verificar", new
        {
            email,
            codigo = Codigo,
            contrasenaNueva = Nueva,
            contrasenaNuevaRepetida = Nueva
        });
        Assert.Equal(HttpStatusCode.NoContent, ok.StatusCode);

        var otra = await cliente.PostAsJsonAsync("/api/auth/cuenta/verificar", new
        {
            email,
            codigo = Codigo,
            contrasenaNueva = "OtraSegura9!",
            contrasenaNuevaRepetida = "OtraSegura9!"
        });
        Assert.Equal(HttpStatusCode.Conflict, otra.StatusCode);
        var json = await Escenario.Json(otra);
        Assert.Equal("auth.cuenta_ya_verificada", json.GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Politica_o_repetir_distinta_devuelve_400()
    {
        var (email, _) = await CrearAccesoPendienteAsync("VerifPol");

        var corta = await api.CreateClient().PostAsJsonAsync("/api/auth/cuenta/verificar", new
        {
            email,
            codigo = Codigo,
            contrasenaNueva = "corta",
            contrasenaNuevaRepetida = "corta"
        });
        Assert.Equal(HttpStatusCode.BadRequest, corta.StatusCode);

        var noCoincide = await api.CreateClient().PostAsJsonAsync("/api/auth/cuenta/verificar", new
        {
            email,
            codigo = Codigo,
            contrasenaNueva = Nueva,
            contrasenaNuevaRepetida = "OtraSegura9!"
        });
        Assert.Equal(HttpStatusCode.BadRequest, noCoincide.StatusCode);
    }

    [Fact]
    public async Task Reenvio_siempre_responde_202_generico()
    {
        var (email, _) = await CrearAccesoPendienteAsync("Reenvio");

        var r = await api.CreateClient().PostAsJsonAsync("/api/auth/cuenta/verificacion/reenvio",
            new { email });
        Assert.Equal(HttpStatusCode.Accepted, r.StatusCode);
        var json = await Escenario.Json(r);
        Assert.Contains("verificación", json.GetProperty("mensaje").GetString(), StringComparison.OrdinalIgnoreCase);

        var inexistente = await api.CreateClient().PostAsJsonAsync("/api/auth/cuenta/verificacion/reenvio",
            new { email = "fantasma@pruebas.hn" });
        Assert.Equal(HttpStatusCode.Accepted, inexistente.StatusCode);
    }

    private async Task<(string Email, string IdEmpresaCifrado)> CrearAccesoPendienteAsync(string nombre)
    {
        using var scope = api.Services.CreateScope();
        var hashCodigo = scope.ServiceProvider.GetRequiredService<IHashCodigoVerificacion>();
        var cifrador = scope.ServiceProvider.GetRequiredService<ICifradorEmpresaId>();

        var empresa = new Empresa { RazonSocial = $"{nombre} S. de R.L.", RTN = RtnUnico(), Estado = EstadoEmpresa.Activa };
        var persona = new Persona { PrimerNombre = "Pendiente", PrimerApellido = nombre };
        var email = $"pendiente.{Guid.NewGuid():N}@pruebas.hn";
        var acceso = new Acceso { Email = email, Verificado = false };
        var vinculo = new VinculoEmpresa
        {
            EmpresaId = empresa.Id,
            PersonaId = persona.Id,
            AccesoId = acceso.Id,
            RolId = Rol.AdminEmpresaId,
            Estado = EstadoVinculo.Activo
        };
        var verificacion = new VerificacionCorreo
        {
            Proposito = PropositoVerificacionCorreo.ActivacionCuenta,
            Email = email,
            AccesoId = acceso.Id,
            CodigoHash = hashCodigo.Hashear(Codigo),
            ExpiraEn = DateTime.UtcNow.AddMinutes(10),
            VentanaEnviosExpiraEn = DateTime.UtcNow.AddMinutes(10)
        };

        await using (var db = api.NuevoContexto(empresaId: null))
        {
            db.AddRange(empresa, persona, new Usuario { PersonaId = persona.Id }, acceso, vinculo, verificacion);
            await db.SaveChangesAsync();
        }

        return (email, cifrador.Cifrar(empresa.Id));
    }

    private static string RtnUnico() => string.Concat(Enumerable.Range(0, 14).Select(_ => Random.Shared.Next(10)));
}
