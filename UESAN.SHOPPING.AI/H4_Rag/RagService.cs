using System.Text;
using Microsoft.Extensions.AI;
using UESAN.SHOPPING.AI.Comun;
using UESAN.SHOPPING.AI.Datos;
using UESAN.SHOPPING.AI.H3_Embeddings;

namespace UESAN.SHOPPING.AI.H4_Rag;

public sealed record FuenteCitada(int Numero, string Fuente, string Seccion, double Similitud);
public sealed record RespuestaRag(string Respuesta, IReadOnlyList<FuenteCitada> Fuentes, bool UsoDatosCliente, long? Tokens);

/// <summary>
/// H4 · RAG (Retrieval-Augmented Generation): recuperar → agregar contexto → generar.
/// Puede combinarse con datos transaccionales (como en el ejemplo de descansos médicos de la clase).
///
/// "RAG amplía lo que la IA puede consultar o saber, pero no le concede automáticamente
///  capacidad para actuar ni decidir el proceso": la secuencia la fija este código.
/// </summary>
public sealed class RagService(IChatClient chat, BuscadorSemantico buscador, TiendaRepositorio repo)
{
    private const double SimilitudMinima = 0.35;

    private const string Sistema = """
        Eres el asistente de atención al cliente de UESAN Shopping.
        Responde SOLO con la información de <documentos> y, si existe, de <datos_cliente>.
        Cita los documentos que uses con su número entre corchetes, por ejemplo [1].
        Si la respuesta no está en esa información, responde exactamente:
        "No tengo esa información en las políticas de la tienda."
        Responde en español, en un máximo de 6 líneas.
        """;

    public async Task<RespuestaRag> PreguntarAsync(string pregunta, int? clienteId, CancellationToken ct)
    {
        // 1. RECUPERAR: fragmentos más parecidos a la pregunta
        var fragmentos = await buscador.BuscarAsync(pregunta, top: 4, similitudMinima: SimilitudMinima, ct: ct);

        // 2. AGREGAR CONTEXTO: documentos numerados (+ datos del cliente si se indicó)
        var prompt = new StringBuilder();
        prompt.AppendLine("<documentos>");
        for (var i = 0; i < fragmentos.Count; i++)
        {
            prompt.AppendLine($"[{i + 1}] ({fragmentos[i].Fuente} › {fragmentos[i].Seccion})");
            prompt.AppendLine(fragmentos[i].Contenido);
            prompt.AppendLine();
        }
        prompt.AppendLine("</documentos>");

        var usoDatosCliente = false;
        if (clienteId is int id)
        {
            var hoy = DateTime.Today;
            var pedidos = await repo.PedidosDeClienteAsync(id);
            prompt.AppendLine("<datos_cliente>");
            prompt.AppendLine(JsonOpciones.Serializar(new
            {
                fechaActual = hoy.ToString("yyyy-MM-dd"),
                pedidos = pedidos.Select(p => new
                {
                    p.Id, p.Estado, diasDesdeEntrega = p.DiasDesdeEntrega(hoy),
                    productos = p.Detalle.Select(d => new { d.ProductoId, d.Producto, d.PrecioUnitario })
                })
            }));
            prompt.AppendLine("</datos_cliente>");
            usoDatosCliente = true;
        }

        prompt.AppendLine().Append("Pregunta: ").AppendLine(pregunta);

        // 3. GENERAR
        List<ChatMessage> mensajes = [new(ChatRole.System, Sistema), new(ChatRole.User, prompt.ToString())];
        var respuesta = await chat.GetResponseAsync(mensajes, new ChatOptions { Temperature = 0.1f }, ct);

        return new RespuestaRag(
            respuesta.Text,
            fragmentos.Select((f, i) => new FuenteCitada(i + 1, f.Fuente, f.Seccion, f.Similitud)).ToList(),
            usoDatosCliente,
            respuesta.Usage?.TotalTokenCount);
    }
}
