using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Norvia.API.Filtros;

/// <summary>
/// Ejecuta el validador de FluentValidation de cada argumento de la acción (si existe uno registrado)
/// y lanza <see cref="ValidationException"/> para que la convierta <c>ManejoErroresMiddleware</c>.
/// Reemplaza al paquete deprecado FluentValidation.AspNetCore.
/// </summary>
public sealed class ValidacionFiltro(IServiceProvider servicios) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argumento in context.ActionArguments.Values)
        {
            if (argumento is null)
                continue;

            var tipoValidador = typeof(IValidator<>).MakeGenericType(argumento.GetType());
            if (servicios.GetService(tipoValidador) is not IValidator validador)
                continue;

            var resultado = await validador.ValidateAsync(
                new ValidationContext<object>(argumento), context.HttpContext.RequestAborted);
            if (!resultado.IsValid)
                throw new ValidationException(resultado.Errors);
        }

        await next();
    }
}
