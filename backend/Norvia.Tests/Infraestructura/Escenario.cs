using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Norvia.Application.Common.Interfaces;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Enums;

namespace Norvia.Tests.Infraestructura;

/// <summary>Una empresa de prueba con su administrador y una sesión abierta.</summary>
public sealed record EmpresaDePrueba(
    Guid EmpresaId, string IdEmpresaCifrado, Guid AdminVinculoId, string AdminEmail, HttpClient Cliente);

/// <summary>
/// Prepara datos y sesiones. Las empresas se crean directo en la base (con los servicios reales de hash y
/// cifrado) para no depender del registro público de HU-017; todo lo que se prueba se hace por HTTP.
/// Cada llamada usa correos y RTN únicos, así las pruebas no dependen unas de otras.
/// </summary>
public sealed class Escenario(NorviaApiFactory api)
{
    public const string PasswordAdmin = "Admin#Pruebas2026";

    public async Task<EmpresaDePrueba> CrearEmpresaAsync(string nombre)
    {
        using var scope = api.Services.CreateScope();
        var hasher = scope.ServiceProvider.GetRequiredService<IHasherPassword>();
        var cifrador = scope.ServiceProvider.GetRequiredService<ICifradorEmpresaId>();

        var empresa = new Empresa { RazonSocial = $"{nombre} S. de R.L.", RTN = RtnUnico(), Estado = EstadoEmpresa.Activa };
        var persona = new Persona { PrimerNombre = "Admin", PrimerApellido = nombre };
        var acceso = new Acceso { Email = $"admin.{Guid.NewGuid():N}@pruebas.hn", Verificado = true };
        acceso.PasswordHash = hasher.Hashear(acceso, PasswordAdmin);
        var vinculo = new VinculoEmpresa
        {
            EmpresaId = empresa.Id, PersonaId = persona.Id, AccesoId = acceso.Id,
            RolId = Rol.AdminEmpresaId, Estado = EstadoVinculo.Activo
        };

        await using (var db = api.NuevoContexto(empresaId: null))
        {
            db.AddRange(empresa, persona, new Usuario { PersonaId = persona.Id }, acceso, vinculo);
            await db.SaveChangesAsync();
        }

        var idCifrado = cifrador.Cifrar(empresa.Id);
        var cliente = await ClienteEmpresaAsync(acceso.Email, PasswordAdmin, idCifrado);
        return new EmpresaDePrueba(empresa.Id, idCifrado, vinculo.Id, acceso.Email, cliente);
    }

    public async Task<HttpResponseMessage> LoginEmpresaAsync(string email, string password, string idEmpresa) =>
        await api.CreateClient().PostAsJsonAsync("/api/auth/login/empresa", new { email, password, idEmpresa });

    public async Task<HttpClient> ClienteEmpresaAsync(string email, string password, string idEmpresa)
    {
        var respuesta = await LoginEmpresaAsync(email, password, idEmpresa);
        respuesta.EnsureSuccessStatusCode();
        return await ConToken(respuesta);
    }

    public async Task<HttpClient> ClienteStaffAsync()
    {
        var respuesta = await api.CreateClient().PostAsJsonAsync("/api/auth/login/plataforma",
            new { email = NorviaApiFactory.StaffEmail, password = NorviaApiFactory.StaffPassword });
        respuesta.EnsureSuccessStatusCode();
        return await ConToken(respuesta);
    }

    /// <summary>Agrega un colaborador con cuenta nueva usando la API (HU-020) y devuelve el id del vínculo.</summary>
    public static async Task<Guid> AgregarColaboradorAsync(EmpresaDePrueba empresa, string email)
    {
        var r = await empresa.Cliente.PostAsJsonAsync("/api/vinculos", new
        {
            email, primerNombre = "Colaborador", primerApellido = "Prueba",
            rolCodigo = "colaborador", passwordTemporal = "Temporal#2026"
        });
        r.EnsureSuccessStatusCode();
        return (await Json(r)).GetProperty("id").GetGuid();
    }

    public static async Task<JsonElement> Json(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync()).RootElement.Clone();

    private async Task<HttpClient> ConToken(HttpResponseMessage login)
    {
        var token = (await Json(login)).GetProperty("accessToken").GetString();
        var cliente = api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }

    private static string RtnUnico() => string.Concat(Enumerable.Range(0, 14).Select(_ => Random.Shared.Next(10)));
}
