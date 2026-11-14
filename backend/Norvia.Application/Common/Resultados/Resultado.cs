namespace Norvia.Application.Common.Resultados;

/// <summary>Resultado de un caso de uso que no devuelve datos.</summary>
public class Resultado
{
    protected Resultado(Error? error) => Error = error;

    public Error? Error { get; }
    public bool EsExito => Error is null;

    public static Resultado Exito() => new(null);
    public static Resultado Fallo(Error error) => new(error);

    public static implicit operator Resultado(Error error) => Fallo(error);
}

/// <summary>Resultado de un caso de uso que devuelve <typeparamref name="T"/> si tiene éxito.</summary>
public sealed class Resultado<T> : Resultado
{
    private readonly T? _valor;

    private Resultado(T valor) : base(null) => _valor = valor;
    private Resultado(Error error) : base(error) { }

    /// <summary>Valor del éxito. Lanza si el resultado es un fallo.</summary>
    public T Valor => EsExito
        ? _valor!
        : throw new InvalidOperationException("Un resultado fallido no tiene valor.");

    public static Resultado<T> Exito(T valor) => new(valor);
    public static new Resultado<T> Fallo(Error error) => new(error);

    public static implicit operator Resultado<T>(T valor) => Exito(valor);
    public static implicit operator Resultado<T>(Error error) => Fallo(error);
}
