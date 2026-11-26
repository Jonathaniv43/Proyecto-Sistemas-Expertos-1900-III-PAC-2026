namespace Norvia.Application.Common.Correo;

/// <summary>Mensaje listo para enviar; no es contrato HTTP.</summary>
public sealed record MensajeCorreo(string Destino, string Asunto, string CuerpoHtml);