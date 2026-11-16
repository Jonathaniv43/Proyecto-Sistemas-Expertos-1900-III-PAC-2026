using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Auth;

namespace Norvia.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("es");

        var ensamblado = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(ensamblado);

        // Un servicio por caso de uso: toda clase concreta que implemente IServicioCasoDeUso.
        var servicios = ensamblado.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IServicioCasoDeUso).IsAssignableFrom(t));
        foreach (var tipo in servicios)
            services.AddScoped(tipo);

        // Piezas compartidas solo por los slices del módulo Auth.
        services.AddScoped<EmisorSesion>();

        // Pieza compartida solo por los slices de alta del módulo Vinculos (HU-020, HU-115).
        services.AddScoped<Features.Vinculos.AltaDeVinculo>();

        return services;
    }
}
