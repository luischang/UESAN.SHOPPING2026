using Microsoft.Extensions.AI;

namespace UESAN.SHOPPING.AI.H5_Herramientas;

public sealed record RespuestaAsistente(string Respuesta, IReadOnlyList<InvocacionHerramienta> Herramientas, long? Tokens);

/// <summary>
/// H5 · Asistente con herramientas (function / tool calling).
/// FunctionInvokingChatClient ejecuta automáticamente las funciones que el modelo pide,
/// pero cada función pasa por la PuertaHerramientas.
/// </summary>
public sealed class AsistenteHerramientasService(IChatClient chat, HerramientasFactory fabrica)
{
    private const string Sistema = """
        Eres el asistente de atención al cliente de UESAN Shopping.
        Usa las herramientas para consultar datos reales antes de responder; nunca inventes datos.
        Si una herramienta devuelve "error", explícale el motivo al cliente con amabilidad.
        Solo registra una devolución si el cliente lo pide de forma explícita.
        Responde en español y en pocas líneas.
        """;

    public async Task<RespuestaAsistente> ResponderAsync(int clienteId, string pregunta, CancellationToken ct)
    {
        var ctx = new ContextoEjecucion
        {
            ClienteId = clienteId,
            Origen = "asistente-h5",
            HerramientasPermitidas = CatalogoHerramientas.Asistente
        };
        var herramientas = fabrica.Crear(ctx);

        // Se envuelve el cliente para que invoque funciones automáticamente (máximo 5 vueltas).
        // No se usa "using": liberarlo liberaría también el cliente compartido.
        var clienteConFunciones = chat.AsBuilder()
            .UseFunctionInvocation(configure: f => f.MaximumIterationsPerRequest = 5)
            .Build();

        List<ChatMessage> mensajes = [new(ChatRole.System, Sistema), new(ChatRole.User, pregunta)];
        var opciones = new ChatOptions { Tools = herramientas.ParaAsistente(), Temperature = 0.1f };

        var respuesta = await clienteConFunciones.GetResponseAsync(mensajes, opciones, ct);
        return new RespuestaAsistente(respuesta.Text, ctx.Invocaciones, respuesta.Usage?.TotalTokenCount);
    }
}
