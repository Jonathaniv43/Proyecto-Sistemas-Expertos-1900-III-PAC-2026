namespace Norvia.Application.Common.CasosDeUso;

/// <summary>
/// Marca el servicio de un caso de uso (un slice). <c>AddApplication</c> registra como scoped
/// toda clase que la implemente. Cada servicio expone un único método <c>EjecutarAsync</c>.
/// </summary>
public interface IServicioCasoDeUso;
