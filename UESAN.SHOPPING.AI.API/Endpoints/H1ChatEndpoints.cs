using Microsoft.Extensions.AI;
using UESAN.SHOPPING.AI.H1_Chat;

namespace UESAN.SHOPPING.AI.API.Endpoints;

public static class H1ChatEndpoints
{
    public static RouteGroupBuilder MapH1Endpoints(this RouteGroupBuilder ia)
    {
        var g = ia.MapGroup("/h1").WithTags("H1 · Pregunta y respuesta con un LLM");

        // ¿Qué modelo estoy usando? (local con Ollama o en la nube)
        g.MapGet("/proveedor", (IChatClient chat) =>
        {
            var meta = chat.GetService<ChatClientMetadata>();
            return new { proveedor = meta?.ProviderName, endpoint = meta?.ProviderUri, modelo = meta?.DefaultModelId };
        });

        g.MapPost("/chat", async (PreguntaRequest req, ChatBasicoService servicio, CancellationToken ct) =>
            await servicio.PreguntarAsync(req.Pregunta, ct));

        // Streaming: el navegador recibe el texto a medida que se genera
        g.MapPost("/chat/stream", (PreguntaRequest req, ChatBasicoService servicio, CancellationToken ct) =>
            servicio.PreguntarEnStreamingAsync(req.Pregunta, ct));

        return ia;
    }
}
