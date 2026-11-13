using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Norvia.Application.Features.Auth;
using Norvia.Application.Features.Auth.Interfaces;

namespace Norvia.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IAuthServicio, AuthServicio>();
        return services;
    }
}
