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

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddControllers(o => o.Filters.Add<ValidacionFiltro>());
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAutenticacionJwt();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantActual, TenantActual>();

var app = builder.Build();

app.UseMiddleware<ManejoErroresMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.Services.InicializarBaseDatosAsync();
app.Run();
