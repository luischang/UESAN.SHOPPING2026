using System.Text.Json;
using Microsoft.Extensions.AI;

namespace UESAN.SHOPPING.AI.Comun;

/// <summary>
/// Pide al modelo una respuesta en JSON y la convierte a un tipo C#.
/// Los modelos pequeños a veces agregan texto o ```json```: se limpia antes de deserializar.
/// Si no se puede interpretar, devuelve null y el CÓDIGO decide qué hacer.
/// </summary>
public static class JsonLlm
{
    public static async Task<T?> PedirAsync<T>(IChatClient chat, string sistema, string usuario, CancellationToken ct)
        where T : class
    {
        var opciones = new ChatOptions { ResponseFormat = ChatResponseFormat.Json, Temperature = 0f };
        List<ChatMessage> mensajes = [new(ChatRole.System, sistema), new(ChatRole.User, usuario)];
        var respuesta = await chat.GetResponseAsync(mensajes, opciones, ct);
        return Parsear<T>(respuesta.Text);
    }

    public static T? Parsear<T>(string? texto) where T : class
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        var inicio = texto.IndexOf('{');
        var fin = texto.LastIndexOf('}');
        if (inicio < 0 || fin <= inicio) return null;

        try
        {
            return JsonSerializer.Deserialize<T>(texto[inicio..(fin + 1)], JsonOpciones.Web);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
