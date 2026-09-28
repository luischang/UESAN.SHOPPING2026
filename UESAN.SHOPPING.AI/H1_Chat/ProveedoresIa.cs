using System.ClientModel;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;
using UESAN.SHOPPING.AI.Configuracion;

namespace UESAN.SHOPPING.AI.H1_Chat;

/// <summary>
/// H1 · Crea el cliente del modelo según la configuración.
/// El resto de la aplicación solo conoce IChatClient / IEmbeddingGenerator
/// (Microsoft.Extensions.AI), así que cambiar de modelo local a nube
/// es cambiar una línea de configuración, no el código.
/// </summary>
public static class ProveedoresIa
{
    public static bool EsOpenAI(AiOptions o) =>
        string.Equals(o.Proveedor, "OpenAI", StringComparison.OrdinalIgnoreCase);

    public static IChatClient CrearChat(AiOptions o) =>
        EsOpenAI(o)
            ? CrearClienteOpenAI(o.OpenAI).GetChatClient(o.OpenAI.ModeloChat).AsIChatClient()
            : new OllamaApiClient(new Uri(o.Ollama.Endpoint), o.Ollama.ModeloChat);

    public static IEmbeddingGenerator<string, Embedding<float>> CrearEmbeddings(AiOptions o) =>
        EsOpenAI(o)
            ? CrearClienteOpenAI(o.OpenAI).GetEmbeddingClient(o.OpenAI.ModeloEmbeddings).AsIEmbeddingGenerator()
            : new OllamaApiClient(new Uri(o.Ollama.Endpoint), o.Ollama.ModeloEmbeddings);

    /// <summary>Identificador del modelo de embeddings activo (se guarda junto a cada vector).</summary>
    public static string ModeloEmbeddingsActivo(AiOptions o) =>
        EsOpenAI(o) ? $"openai:{o.OpenAI.ModeloEmbeddings}" : $"ollama:{o.Ollama.ModeloEmbeddings}";

    private static OpenAIClient CrearClienteOpenAI(OpenAIOptions o)
    {
        if (string.IsNullOrWhiteSpace(o.ApiKey))
            throw new InvalidOperationException(
                "Configura AI:OpenAI:ApiKey con 'dotnet user-secrets' o la variable de entorno AI__OpenAI__ApiKey.");

        var opciones = new OpenAIClientOptions();
        if (!string.IsNullOrWhiteSpace(o.Endpoint)) opciones.Endpoint = new Uri(o.Endpoint);
        return new OpenAIClient(new ApiKeyCredential(o.ApiKey), opciones);
    }
}
