using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace UESAN.SHOPPING.AI.H1_Chat;

public sealed record RespuestaChat(string Respuesta, string? Modelo, long Milisegundos, long? Tokens);

/// <summary>
/// H1 · Aplicación de pregunta y respuesta.
/// Usuario → aplicación → API del modelo → respuesta.
/// No consulta datos, no usa herramientas y no decide pasos: NO es un agente.
/// </summary>
public sealed class ChatBasicoService(IChatClient chat)
{
    private const string Sistema = """
        Eres el asistente virtual de UESAN Shopping, una tienda en línea peruana.
        Responde en español, de forma breve y amable.
        """;

    public async Task<RespuestaChat> PreguntarAsync(string pregunta, CancellationToken ct)
    {
        var cronometro = Stopwatch.StartNew();

        List<ChatMessage> mensajes =
        [
            new(ChatRole.System, Sistema),
            new(ChatRole.User, pregunta)
        ];

        ChatResponse respuesta = await chat.GetResponseAsync(mensajes, cancellationToken: ct);

        return new RespuestaChat(
            respuesta.Text,
            respuesta.ModelId,
            cronometro.ElapsedMilliseconds,
            respuesta.Usage?.TotalTokenCount);
    }

    /// <summary>Misma consulta, pero devolviendo el texto a medida que el modelo lo genera.</summary>
    public async IAsyncEnumerable<string> PreguntarEnStreamingAsync(
        string pregunta, [EnumeratorCancellation] CancellationToken ct)
    {
        List<ChatMessage> mensajes =
        [
            new(ChatRole.System, Sistema),
            new(ChatRole.User, pregunta)
        ];

        await foreach (var fragmento in chat.GetStreamingResponseAsync(mensajes, cancellationToken: ct))
        {
            if (!string.IsNullOrEmpty(fragmento.Text)) yield return fragmento.Text;
        }
    }
}
