using System.ComponentModel;
using Microsoft.Extensions.AI;
using UESAN.SHOPPING.AI.Datos;
using UESAN.SHOPPING.AI.H3_Embeddings;

namespace UESAN.SHOPPING.AI.H5_Herramientas;

/// <summary>
/// H5 · Herramientas que el modelo puede SOLICITAR. Cada método:
///  - tiene una descripción clara (el modelo decide con base en ella),
///  - pasa por la PuertaHerramientas,
///  - valida y autoriza con el cliente del contexto (nunca con un id que "diga" el modelo).
/// Informar: ConsultarProducto, ConsultarPedido, BuscarPolitica.
/// Actuar:   RegistrarDevolucion.
/// </summary>
public sealed class HerramientasTienda(
    ContextoEjecucion ctx,
    PuertaHerramientas puerta,
    TiendaRepositorio tienda,
    BuscadorSemantico buscador,
    RepositorioDevoluciones devoluciones)
{
    public ContextoEjecucion Contexto => ctx;

    [Description("Busca productos del catálogo por nombre, descripción o categoría. Devuelve id, nombre, precio y stock.")]
    public Task<string> ConsultarProducto(
        [Description("Texto a buscar, por ejemplo 'audífonos' o 'libros'")] string texto) =>
        puerta.EjecutarAsync(ctx, nameof(ConsultarProducto), new { texto }, async () =>
        {
            if (string.IsNullOrWhiteSpace(texto) || texto.Length > 100)
                throw new ReglaNegocioException("El texto de búsqueda debe tener entre 1 y 100 caracteres.");
            return await tienda.BuscarProductosAsync(texto);
        });

    [Description("Obtiene un pedido del cliente autenticado: estado, fecha de entrega, días desde la entrega y productos (con productoId).")]
    public Task<string> ConsultarPedido(
        [Description("Número de pedido, por ejemplo 1001")] int pedidoId) =>
        puerta.EjecutarAsync(ctx, nameof(ConsultarPedido), new { pedidoId }, async () =>
        {
            var p = await PedidoDelClienteAsync(pedidoId);
            return new
            {
                p.Id, p.Estado,
                fecha = p.Fecha.ToString("yyyy-MM-dd"),
                fechaEntrega = p.FechaEntrega?.ToString("yyyy-MM-dd"),
                diasDesdeEntrega = p.DiasDesdeEntrega(DateTime.Today),
                p.Total,
                productos = p.Detalle.Select(d => new { d.ProductoId, d.Producto, d.Cantidad, d.PrecioUnitario })
            };
        });

    [Description("Busca en las políticas de la tienda (devoluciones, envíos, garantías, pagos). Úsala antes de afirmar una regla.")]
    public Task<string> BuscarPolitica(
        [Description("Consulta en lenguaje natural, por ejemplo 'plazo para devolver'")] string consulta) =>
        puerta.EjecutarAsync(ctx, nameof(BuscarPolitica), new { consulta }, async () =>
            (await buscador.BuscarAsync(consulta, top: 3, similitudMinima: 0.35))
                .Select(r => new { r.Fuente, r.Seccion, r.Contenido }));

    [Description("ACCIÓN: registra una solicitud de devolución. Solo procede si el pedido está ENTREGADO, dentro de 30 días y el producto es retornable. Úsala solo si el cliente pidió devolver.")]
    public Task<string> RegistrarDevolucion(
        [Description("Número de pedido")] int pedidoId,
        [Description("Id del producto dentro del pedido (productoId que devuelve ConsultarPedido)")] int productoId,
        [Description("Motivo de la devolución indicado por el cliente")] string motivo) =>
        puerta.EjecutarAsync(ctx, nameof(RegistrarDevolucion), new { pedidoId, productoId, motivo }, async () =>
        {
            if (string.IsNullOrWhiteSpace(motivo))
                throw new ReglaNegocioException("El motivo es obligatorio.");

            var pedido = await PedidoDelClienteAsync(pedidoId);
            var retornable = await devoluciones.EsRetornableAsync(productoId);
            ReglasDevolucion.Validar(pedido, productoId, retornable, DateTime.Today);

            var (solicitud, yaExistia) = await devoluciones.RegistrarAsync(
                pedidoId, productoId, ctx.ClienteId, motivo, ctx.Origen);

            return new
            {
                solicitudId = solicitud.Id,
                solicitud.Estado,
                yaExistia,
                mensaje = yaExistia
                    ? "Ya existía una solicitud para este producto (operación idempotente)."
                    : "Solicitud de devolución registrada."
            };
        });

    // ───────────── H7 · herramientas adicionales del agente ─────────────

    [Description("ACCIÓN: emite el reembolso de una solicitud de devolución REGISTRADA. El monto no puede superar lo pagado por el producto. Montos mayores a S/ 500 requieren aprobación de un supervisor.")]
    public Task<string> EmitirReembolso(
        [Description("Id de la solicitud de devolución (solicitudId que devuelve RegistrarDevolucion)")] int solicitudId,
        [Description("Monto a reembolsar en soles")] decimal monto) =>
        puerta.EjecutarAsync(ctx, nameof(EmitirReembolso), new { solicitudId, monto }, async () =>
        {
            var solicitud = await devoluciones.ObtenerAsync(solicitudId)
                ?? throw new ReglaNegocioException($"No existe la solicitud {solicitudId}.");
            if (solicitud.ClienteId != ctx.ClienteId)
                throw new ReglaNegocioException("La solicitud no pertenece al cliente autenticado.");
            if (solicitud.Estado == "REEMBOLSO_EMITIDO")
                return new { solicitudId, solicitud.Estado, solicitud.MontoReembolso, mensaje = "El reembolso ya había sido emitido (idempotente)." };
            if (solicitud.Estado != "REGISTRADA")
                throw new ReglaNegocioException($"La solicitud está en estado {solicitud.Estado}.");

            var pedido = await tienda.ObtenerPedidoAsync(solicitud.PedidoId);
            var pagado = pedido?.Detalle.Where(d => d.ProductoId == solicitud.ProductoId)
                .Sum(d => d.PrecioUnitario * d.Cantidad) ?? 0m;
            if (monto <= 0 || monto > pagado)
                throw new ReglaNegocioException($"El monto debe ser mayor que 0 y no superar lo pagado (S/ {pagado:0.00}).");

            // Control en CÓDIGO (no en el prompt): acción sensible sin aprobación → no se ejecuta
            if (monto > ctx.UmbralAprobacion && !ctx.Aprobaciones.Contains(ClaveAprobacion(solicitudId)))
                throw new ReglaNegocioException("Este reembolso requiere aprobación de un supervisor.");

            await devoluciones.EmitirReembolsoAsync(solicitudId, monto);
            return new { solicitudId, estado = "REEMBOLSO_EMITIDO", monto };
        });

    [Description("Pide al cliente una foto o video del producto defectuoso. El agente queda EN ESPERA hasta que el cliente la envíe.")]
    public string SolicitarEvidencia(
        [Description("Qué evidencia necesitas y por qué")] string motivo) =>
        "El agente queda en espera de la evidencia.";   // el ciclo del agente intercepta esta herramienta

    [Description("Deriva el caso a una persona de postventa cuando es ambiguo, conflictivo o está fuera de la política.")]
    public string EscalarAHumano(
        [Description("Motivo del escalamiento")] string motivo) =>
        "Caso escalado.";                               // el ciclo del agente intercepta esta herramienta

    public static string ClaveAprobacion(int solicitudId) => $"EmitirReembolso:{solicitudId}";

    /// <summary>Herramientas que se ofrecen al agente de H7 (por nombre, para ejecutarlas bajo control).</summary>
    public IReadOnlyDictionary<string, AIFunction> ParaAgente()
    {
        AIFunction[] funciones =
        [
            AIFunctionFactory.Create(ConsultarPedido),
            AIFunctionFactory.Create(ConsultarProducto),
            AIFunctionFactory.Create(BuscarPolitica),
            AIFunctionFactory.Create(RegistrarDevolucion),
            AIFunctionFactory.Create(EmitirReembolso),
            AIFunctionFactory.Create(SolicitarEvidencia),
            AIFunctionFactory.Create(EscalarAHumano)
        ];
        return funciones.ToDictionary(f => f.Name);
    }

    /// <summary>Herramientas que se ofrecen al asistente de H5.</summary>
    public IList<AITool> ParaAsistente() =>
    [
        AIFunctionFactory.Create(ConsultarProducto),
        AIFunctionFactory.Create(ConsultarPedido),
        AIFunctionFactory.Create(BuscarPolitica),
        AIFunctionFactory.Create(RegistrarDevolucion)
    ];

    /// <summary>Autorización: el pedido debe pertenecer al cliente del contexto.</summary>
    private async Task<PedidoDto> PedidoDelClienteAsync(int pedidoId)
    {
        var pedido = await tienda.ObtenerPedidoAsync(pedidoId)
            ?? throw new ReglaNegocioException($"No existe el pedido {pedidoId}.");
        if (pedido.ClienteId != ctx.ClienteId)
            throw new ReglaNegocioException("El pedido no pertenece al cliente autenticado.");
        return pedido;
    }
}

/// <summary>Crea las herramientas para un contexto (una instancia por solicitud).</summary>
public sealed class HerramientasFactory(
    PuertaHerramientas puerta, TiendaRepositorio tienda, BuscadorSemantico buscador, RepositorioDevoluciones devoluciones)
{
    public HerramientasTienda Crear(ContextoEjecucion ctx) => new(ctx, puerta, tienda, buscador, devoluciones);
}
