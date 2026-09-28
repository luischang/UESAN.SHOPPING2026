using UESAN.SHOPPING.AI.H4_Rag;

namespace UESAN.SHOPPING.AI.API.Endpoints;

public static class H4RagEndpoints
{
    public static RouteGroupBuilder MapH4Endpoints(this RouteGroupBuilder ia)
    {
        var g = ia.MapGroup("/h4").WithTags("H4 · RAG");

        g.MapPost("/rag", async (PreguntaRagRequest req, RagService rag, CancellationToken ct) =>
            await rag.PreguntarAsync(req.Pregunta, req.ClienteId, ct));

        return ia;
    }
}
