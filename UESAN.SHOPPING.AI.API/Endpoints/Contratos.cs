namespace UESAN.SHOPPING.AI.API.Endpoints;

// Cuerpos de las solicitudes de los endpoints de IA.
public sealed record PreguntaRequest(string Pregunta);
public sealed record PreguntaClienteRequest(int ClienteId, string Pregunta);
