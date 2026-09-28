using UESAN.SHOPPING.AI.H3_Embeddings;

namespace UESAN.SHOPPING.AI.API.Endpoints;

public static class H3EmbeddingsEndpoints
{
    public static RouteGroupBuilder MapH3Endpoints(this RouteGroupBuilder ia)
    {
        var g = ia.MapGroup("/h3").WithTags("H3 · Embeddings y búsqueda vectorial");

        g.MapGet("/embedding", async (string texto, BuscadorSemantico buscador, CancellationToken ct) =>
            await buscador.VerEmbeddingAsync(texto, ct));

        g.MapGet("/similitud", async (string a, string b, BuscadorSemantico buscador, CancellationToken ct) =>
            new { a, b, similitud = await buscador.SimilitudAsync(a, b, ct) });

        g.MapPost("/ingesta", async (IngestaService ingesta, CancellationToken ct) =>
            await ingesta.IndexarCarpetaAsync(ct));

        g.MapGet("/indice", async (RepositorioConocimiento repo) => await repo.ResumenAsync());

        g.MapGet("/buscar", async (string q, int? top, BuscadorSemantico buscador, CancellationToken ct) =>
            await buscador.BuscarAsync(q, top ?? 3, ct: ct));

        return ia;
    }
}
