using Microsoft.Extensions.AI;
using UESAN.SHOPPING.AI.Comun;
using UESAN.SHOPPING.AI.Datos;

namespace UESAN.SHOPPING.AI.H2_DatosTransaccionales;

public sealed record RespuestaConContexto(string Respuesta, string ContextoEnviado, long? Tokens);

/// <summary>
/// H2 · Aplicación enriquecida con DATOS TRANSACCIONALES.
/// El CÓDIGO decide qué consultar (los pedidos del cliente), arma el contexto y llama al modelo.
/// El modelo solo redacta. La secuencia sigue siendo fija: esto tampoco es un agente.
/// </summary>
public sealed class AsistentePedidosService(IChatClient chat, TiendaRepositorio repo)
{
    private const string Sistema = """
        Eres el asistente de pedidos de UESAN Shopping.
        Responde SOLO con la información de <datos_cliente>. Si el dato no está, dilo claramente.
        No inventes fechas, montos ni estados. Responde en español y en pocas líneas.
        """;

    public async Task<RespuestaConContexto> ResponderAsync(int clienteId, string pregunta, CancellationToken ct)
    {
        // 1. El código consulta la base de datos (consulta exacta, con el id del cliente)
        var cliente = await repo.ObtenerClienteAsync(clienteId)
            ?? throw new KeyNotFoundException($"No existe el cliente {clienteId}.");
        var pedidos = await repo.PedidosDeClienteAsync(clienteId);

        // 2. El código arma el contexto (solo los campos necesarios: minimización de datos)
        var hoy = DateTime.Today;
        var contexto = JsonOpciones.Serializar(new
        {
            fechaActual = hoy.ToString("yyyy-MM-dd"),
            cliente = new { cliente.Id, cliente.Nombres },
            pedidos = pedidos.Select(p => new
            {
                p.Id,
                fecha = p.Fecha.ToString("yyyy-MM-dd"),
                p.Estado,
                fechaEntrega = p.FechaEntrega?.ToString("yyyy-MM-dd"),
                diasDesdeEntrega = p.DiasDesdeEntrega(hoy),
                p.Total,
                productos = p.Detalle.Select(d => new { d.ProductoId, d.Producto, d.Cantidad, d.PrecioUnitario })
            })
        });

        // 3. El modelo redacta la respuesta con ese contexto
        List<ChatMessage> mensajes =
        [
            new(ChatRole.System, Sistema),
            new(ChatRole.User, $"<datos_cliente>\n{contexto}\n</datos_cliente>\n\nPregunta: {pregunta}")
        ];

        var respuesta = await chat.GetResponseAsync(mensajes, new ChatOptions { Temperature = 0.2f }, ct);
        return new RespuestaConContexto(respuesta.Text, contexto, respuesta.Usage?.TotalTokenCount);
    }
}
