using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Norvia.API.Binding;

/// <summary>
/// Enums en la ruta o la consulta (p. ej. <c>?estado=en_progreso</c>) en snake_case, igual que en el JSON y en la BD.
/// El binder por defecto solo entiende el nombre del enum (<c>EnProgreso</c>) y además acepta números; este rechaza
/// los números y los valores desconocidos con un error de validación (400).
/// </summary>
public sealed class EnumSnakeCaseBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext contexto)
    {
        var valor = contexto.ValueProvider.GetValue(contexto.ModelName);
        if (valor == ValueProviderResult.None) return Task.CompletedTask;
        contexto.ModelState.SetModelValue(contexto.ModelName, valor);

        var texto = valor.FirstValue?.Trim();
        if (string.IsNullOrEmpty(texto)) return Task.CompletedTask; // vacío = sin filtro

        var tipo = Nullable.GetUnderlyingType(contexto.ModelType) ?? contexto.ModelType;
        var nombre = texto.Replace("_", string.Empty);
        if (!char.IsDigit(texto[0]) && Enum.TryParse(tipo, nombre, ignoreCase: true, out var resultado) && Enum.IsDefined(tipo, resultado!))
        {
            contexto.Result = ModelBindingResult.Success(resultado);
            return Task.CompletedTask;
        }

        contexto.ModelState.TryAddModelError(contexto.ModelName, $"El valor '{texto}' no es válido.");
        contexto.Result = ModelBindingResult.Failed();
        return Task.CompletedTask;
    }
}

/// <summary>Usa <see cref="EnumSnakeCaseBinder"/> para todo enum (o enum nullable) que no venga en el cuerpo.</summary>
public sealed class EnumSnakeCaseBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext contexto)
    {
        var tipo = Nullable.GetUnderlyingType(contexto.Metadata.ModelType) ?? contexto.Metadata.ModelType;
        return tipo.IsEnum ? new EnumSnakeCaseBinder() : null;
    }
}
