namespace UESAN.SHOPPING.AI.API.Endpoints;

// Cuerpos de las solicitudes de los endpoints de IA.
public sealed record PreguntaRequest(string Pregunta);
public sealed record PreguntaClienteRequest(int ClienteId, string Pregunta);
public sealed record PreguntaRagRequest(string Pregunta, int? ClienteId);
public sealed record SolicitudDevolucionRequest(int PedidoId, string Mensaje);
public sealed record IniciarAgenteRequest(string Mensaje);
public sealed record EvidenciaRequest(string Descripcion);
public sealed record AprobacionRequest(string Aprobador);
public sealed record EncolarEventoRequest(int ClienteId, string Mensaje);
