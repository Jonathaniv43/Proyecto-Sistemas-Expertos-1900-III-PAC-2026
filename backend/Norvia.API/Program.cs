using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetEnv;
using Norvia.API.Binding;
using Norvia.API.Extensiones;
using Norvia.API.Filtros;
using Norvia.API.Middleware;
using Norvia.API.SegundoPlano;
using Norvia.API.Seguridad;
using Norvia.Application.Common.Interfaces;
using Norvia.Application;
using Norvia.Infrastructure;
using Norvia.Infrastructure.Persistence.Seed;
using Scalar.AspNetCore;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddControllers(o =>
    {
        o.Filters.Add<ValidacionFiltro>();
        // Enums de la consulta en snake_case, como en el JSON: ?estado=en_progreso.
        o.ModelBinderProviders.Insert(0, new EnumSnakeCaseBinderProvider());
    })
    // Enums como texto en snake_case, igual que en la BD: "seleccion_unica", "ergonomico"…
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false)));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAutenticacionJwt();
builder.Services.AddCorsFrontend();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantActual, TenantActual>();
// HU-118: bandeja de salida del correo de las notificaciones, con reintentos.
builder.Services.AddHostedService<ProcesadorCorreosPendientes>();

var app = builder.Build();

app.UseMiddleware<ManejoErroresMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.UseHttpsRedirection();
// CORS antes de la autenticación y de los permisos: responde las preflight OPTIONS sin que lleguen
// a AutorizacionPermisosMiddleware, y agrega los encabezados CORS también a los 401/403.
app.UseCors();
app.UseAuthentication();
app.UseMiddleware<AutorizacionPermisosMiddleware>();
app.UseAuthorization();
app.MapControllers();

await app.Services.InicializarBaseDatosAsync();
app.Run();

/// <summary>Visible para las pruebas de integración (WebApplicationFactory, HU-098).</summary>
public partial class Program;
