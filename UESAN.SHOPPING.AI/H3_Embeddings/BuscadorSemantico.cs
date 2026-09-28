using System.Numerics.Tensors;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using UESAN.SHOPPING.AI.Configuracion;
using UESAN.SHOPPING.AI.H1_Chat;

namespace UESAN.SHOPPING.AI.H3_Embeddings;

public sealed record ResultadoBusqueda(string Fuente, string Seccion, string Contenido, double Similitud);

/// <summary>
/// H3 · Búsqueda vectorial: embedding de la consulta + similitud coseno contra cada fragmento.
/// Para un curso basta recorrer los fragmentos en memoria; en producción se usa un
/// almacén vectorial con índices (Azure AI Search, Qdrant, pgvector, SQL Server 2025...).
/// </summary>
public sealed class BuscadorSemantico(
    IEmbeddingGenerator<string, Embedding<float>> generador,
    RepositorioConocimiento repositorio,
    IOptions<AiOptions> opciones)
{
    public async Task<IReadOnlyList<ResultadoBusqueda>> BuscarAsync(
        string consulta, int top = 3, double similitudMinima = 0.0, CancellationToken ct = default)
    {
        var modelo = ProveedoresIa.ModeloEmbeddingsActivo(opciones.Value);
        ReadOnlyMemory<float> vectorConsulta = await generador.GenerateVectorAsync(consulta, cancellationToken: ct);
        var fragmentos = await repositorio.ListarAsync(modelo);

        return fragmentos
            .Where(f => f.Dimensiones == vectorConsulta.Length)
            .Select(f => new ResultadoBusqueda(
                f.Fuente, f.Seccion, f.Contenido,
                Math.Round(TensorPrimitives.CosineSimilarity(vectorConsulta.Span, f.Vector), 4)))
            .Where(r => r.Similitud >= similitudMinima)
            .OrderByDescending(r => r.Similitud)
            .Take(top)
            .ToList();
    }

    /// <summary>Para la clase: similitud entre dos textos cualesquiera.</summary>
    public async Task<double> SimilitudAsync(string a, string b, CancellationToken ct)
    {
        var vectores = await generador.GenerateAsync([a, b], cancellationToken: ct);
        return Math.Round(TensorPrimitives.CosineSimilarity(vectores[0].Vector.Span, vectores[1].Vector.Span), 4);
    }

    /// <summary>Para la clase: cómo se ve un embedding.</summary>
    public async Task<object> VerEmbeddingAsync(string texto, CancellationToken ct)
    {
        ReadOnlyMemory<float> v = await generador.GenerateVectorAsync(texto, cancellationToken: ct);
        return new
        {
            texto,
            modelo = ProveedoresIa.ModeloEmbeddingsActivo(opciones.Value),
            dimensiones = v.Length,
            primerosValores = v[..Math.Min(8, v.Length)].ToArray()
        };
    }
}
