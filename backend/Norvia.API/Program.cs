using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetEnv;
using Norvia.API.Extensiones;
using Norvia.API.Filtros;
using Norvia.API.Middleware;
using Norvia.API.Seguridad;
using Norvia.Application.Common.Interfaces;
using Norvia.Application;
using Norvia.Infrastructure;
using Norvia.Infrastructure.Persistence.Seed;
using Scalar.AspNetCore;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgresConnection") 
                       ?? throw new InvalidOperationException("Falta la cadena de conexión a PostgreSQL.");

// Leemos los orígenes permitidos desde el archivo .env del Backend
var corsAllowedOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS") 
                         ?? throw new InvalidOperationException("Falta la variable CORS_ALLOWED_ORIGINS en el archivo .env.");
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddControllers(o => o.Filters.Add<ValidacionFiltro>())
    // Enums como texto en snake_case, igual que en la BD: "seleccion_unica", "ergonomico"…
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false)));
builder.Services.AddApplication();
builder.Services.AddCors(options =>
{
    options.AddPolicy("PoliticaCorsNorvia", policy =>
    {
        // Separamos por coma por si existen múltiples orígenes (ej. localhost + https://*.unah.edu.hn)
        var origenes = corsAllowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        policy.WithOrigins(origenes)
              .SetIsOriginAllowedToAllowWildcardSubdomains() // Habilita comodines de subdominio (ej. https://*.unah.edu.hn)
              .AllowAnyHeader()                              // Autoriza encabezados JSON y Tokens JWT
              .AllowAnyMethod()                              // Autoriza verbos GET y Preflights (POST, PUT, DELETE)
              .AllowCredentials();                           // Cookies HttpOnly de sesión (refresh / access)
    });
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAutenticacionJwt();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<CookieSesion>();
builder.Services.AddScoped<ITenantActual, TenantActual>();

var app = builder.Build();

app.UseMiddleware<ManejoErroresMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<AutorizacionPermisosMiddleware>(); //este debe ir antes del middleware (app.UseCors()), para que las preflight OPTIONS no caigan aquÃ­.
app.UseAuthorization();
app.UseCors("PoliticaCorsNorvia");
app.MapControllers();

await app.Services.InicializarBaseDatosAsync();
app.Run();

/// <summary>Visible para las pruebas de integración (WebApplicationFactory, HU-098).</summary>
public partial class Program;
