namespace Norvia.Application.Common.Correo;

/// <param name="ClaveIdempotencia">Si se repite en un reenvío, el proveedor no manda el correo dos veces.</param>
public sealed record MensajeCorreo(string Destino, string Asunto, string CuerpoHtml, string? ClaveIdempotencia = null);
