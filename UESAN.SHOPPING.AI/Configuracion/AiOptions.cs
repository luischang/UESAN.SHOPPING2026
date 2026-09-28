namespace UESAN.SHOPPING.AI.Configuracion;

/// <summary>
/// Opciones de la sección "AI" del appsettings.json.
/// </summary>
public sealed class AiOptions
{
    public const string Seccion = "AI";

    /// <summary>
    /// "Ollama" = modelo LOCAL (corre en tu máquina o en Docker).
    /// "OpenAI" = servicio en la NUBE (OpenAI, Azure OpenAI o cualquier endpoint compatible).
    /// </summary>
    public string Proveedor { get; set; } = "Ollama";

    public OllamaOptions Ollama { get; set; } = new();
    public OpenAIOptions OpenAI { get; set; } = new();
}

public sealed class OllamaOptions
{
    public string Endpoint { get; set; } = "http://localhost:11434";
    public string ModeloChat { get; set; } = "qwen2.5:3b";
    public string ModeloEmbeddings { get; set; } = "bge-m3";
}

public sealed class OpenAIOptions
{
    /// <summary>Nunca en el appsettings: usar user-secrets o variable de entorno AI__OpenAI__ApiKey.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Opcional: endpoint compatible con OpenAI (Azure, GitHub Models, etc.).</summary>
    public string? Endpoint { get; set; }

    public string ModeloChat { get; set; } = "gpt-4.1-mini";
    public string ModeloEmbeddings { get; set; } = "text-embedding-3-small";
}
