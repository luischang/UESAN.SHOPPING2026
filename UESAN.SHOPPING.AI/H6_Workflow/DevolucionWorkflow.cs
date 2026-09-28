using Microsoft.Extensions.AI;
using UESAN.SHOPPING.AI.Comun;
using UESAN.SHOPPING.AI.Datos;
using UESAN.SHOPPING.AI.H5_Herramientas;

namespace UESAN.SHOPPING.AI.H6_Workflow;

public sealed record PasoWorkflow(int Numero, string Paso, string Responsable, string Resultado);
public sealed record ResultadoWorkflow(string Estado, int? SolicitudId, string MensajeAlCliente, IReadOnlyList<PasoWorkflow> Pasos);

public sealed record ClasificacionSolicitud(string? Tipo);
public sealed record DatosSolicitud(string? Producto, string? Motivo);

/// <summary>
/// H6 · WORKFLOW CON IA para solicitudes de devolución.
///
///   Clasificar (LLM) → Validar pedido (código) → Extraer datos (LLM) → Resolver producto (código)
///   → Validar política (código) → Registrar (código) → Redactar notificación (LLM)
///
/// El camino está escrito aquí. El LLM clasifica, extrae y redacta DENTRO de pasos fijos.
/// "Un workflow puede utilizar IA sin delegar al modelo el control del proceso."
/// En la terminología de Anthropic ("Building effective agents"), esto es un
/// "prompt chaining" con verificaciones programáticas ("gates") entre pasos.
/// </summary>
public sealed class DevolucionWorkflow(IChatClient chat, TiendaRepositorio tienda, RepositorioDevoluciones devoluciones)
{
    public async Task<ResultadoWorkflow> EjecutarAsync(int clienteId, int pedidoId, string mensaje, CancellationToken ct)
    {
        var pasos = new List<PasoWorkflow>();
        void Paso(string nombre, string responsable, string resultado) =>
            pasos.Add(new PasoWorkflow(pasos.Count + 1, nombre, responsable, resultado));
        ResultadoWorkflow Fin(string estado, int? solicitudId, string mensajeCliente) =>
            new(estado, solicitudId, mensajeCliente, pasos);

        // 1 · Clasificar (LLM)
        var clasificacion = await JsonLlm.PedirAsync<ClasificacionSolicitud>(chat,
            """
            Clasifica el mensaje de un cliente de una tienda en línea.
            Responde SOLO con JSON: {"tipo": "DEVOLUCION" | "RECLAMO_ENVIO" | "CONSULTA" | "OTRO"}
            """, mensaje, ct);
        var tipo = clasificacion?.Tipo?.Trim().ToUpperInvariant() ?? "OTRO";
        Paso("Clasificar solicitud", "LLM", tipo);
        if (tipo != "DEVOLUCION")
            return Fin("DERIVADA", null, $"Tu mensaje fue clasificado como {tipo}. Esta ruta solo atiende devoluciones.");

        // 2 · Validar pedido (código)
        var pedido = await tienda.ObtenerPedidoAsync(pedidoId);
        if (pedido is null || pedido.ClienteId != clienteId)
        {
            Paso("Validar pedido", "Código", "Pedido inexistente o de otro cliente");
            return Fin("RECHAZADA", null, "No encontramos ese pedido en tu cuenta.");
        }
        Paso("Validar pedido", "Código", $"Pedido {pedido.Id} en estado {pedido.Estado}");

        // 3 · Extraer producto y motivo (LLM)
        var productos = string.Join(", ", pedido.Detalle.Select(d => d.Producto));
        var datos = await JsonLlm.PedirAsync<DatosSolicitud>(chat,
            $$"""
            Extrae del mensaje del cliente el producto que quiere devolver y el motivo.
            Productos del pedido: {{productos}}
            Responde SOLO con JSON: {"producto": "<nombre de la lista o null>", "motivo": "<motivo breve>"}
            """, mensaje, ct);
        Paso("Extraer producto y motivo", "LLM", JsonOpciones.Serializar(datos));

        // 4 · Resolver el producto dentro del pedido (código)
        var detalle = pedido.Detalle.Count == 1
            ? pedido.Detalle[0]
            : pedido.Detalle.FirstOrDefault(d => datos?.Producto is { } nombre &&
                (d.Producto.Contains(nombre, StringComparison.OrdinalIgnoreCase) ||
                 nombre.Contains(d.Producto, StringComparison.OrdinalIgnoreCase)));
        if (detalle is null)
        {
            Paso("Resolver producto", "Código", "No se identificó el producto");
            return Fin("INCOMPLETA", null, $"Indica qué producto del pedido deseas devolver: {productos}.");
        }
        Paso("Resolver producto", "Código", $"{detalle.Producto} (id {detalle.ProductoId})");

        // 5 · Validar política (código)
        try
        {
            var retornable = await devoluciones.EsRetornableAsync(detalle.ProductoId);
            ReglasDevolucion.Validar(pedido, detalle.ProductoId, retornable, DateTime.Today);
            Paso("Validar política", "Código", "Cumple plazo, estado y producto retornable");
        }
        catch (ReglaNegocioException ex)
        {
            Paso("Validar política", "Código", ex.Message);
            return Fin("RECHAZADA", null, $"No podemos registrar la devolución: {ex.Message}");
        }

        // 6 · Registrar (código)
        var (solicitud, yaExistia) = await devoluciones.RegistrarAsync(
            pedido.Id, detalle.ProductoId, clienteId, datos?.Motivo ?? mensaje, "workflow-h6");
        Paso("Registrar solicitud", "Código", yaExistia ? $"Ya existía la solicitud {solicitud.Id}" : $"Solicitud {solicitud.Id} registrada");

        // 7 · Redactar notificación (LLM) — el código la enviaría; aquí solo se devuelve
        List<ChatMessage> redaccion =
        [
            new(ChatRole.System, "Redacta un mensaje breve (máximo 4 líneas), amable y en español para el cliente. Usa solo los datos dados."),
            new(ChatRole.User, JsonOpciones.Serializar(new
            {
                cliente = pedido.Cliente, solicitudId = solicitud.Id, producto = detalle.Producto,
                pedido = pedido.Id, plazoReembolso = "5 a 10 días hábiles tras la revisión en almacén"
            }))
        ];
        var notificacion = await chat.GetResponseAsync(redaccion, cancellationToken: ct);
        Paso("Redactar notificación", "LLM", "Mensaje generado");

        return Fin("REGISTRADA", solicitud.Id, notificacion.Text);
    }
}
