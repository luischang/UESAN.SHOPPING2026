using Microsoft.Extensions.AI;
using UESAN.SHOPPING.AI.Comun;
using UESAN.SHOPPING.AI.H3_Embeddings;
using UESAN.SHOPPING.AI.H5_Herramientas;

namespace UESAN.SHOPPING.AI.H10_Multiagente;

public sealed record RespuestaTrabajador(string Especialidad, string Respuesta);
public sealed record RespuestaOrquestada(string Respuesta, IReadOnlyList<RespuestaTrabajador> Trabajadores);

/// <summary>
/// H10 · MULTIAGENTE: patrón orquestador-trabajadores.
///
/// IMPORTANTE (verificado en "Building effective agents" de Anthropic): este patrón
/// SÍ lo describe Anthropic, pero lo clasifica como un WORKFLOW (orchestrator-workers),
/// no como un tipo de "agente" distinto. La diferencia frente a H6 es que aquí el
/// orquestador (un LLM) decide EN TIEMPO DE EJECUCIÓN qué trabajadores activar y con
/// qué subtarea, en vez de seguir una secuencia fija escrita en el código.
///
/// "Agentic AI" como sinónimo de "sistema multiagente" es una interpretación DIDÁCTICA
/// de este curso para ordenar la progresión de temas — Anthropic no usa esa equivalencia.
/// </summary>
public sealed class OrquestadorPostventaService(
    IChatClient chat, BuscadorSemantico buscador, TiendaRepositorio tienda)
{
    private const string SistemaOrquestador = """
        Eres el orquestador de atención al cliente de UESAN Shopping.
        Divide la pregunta del cliente en subtareas para hasta 3 trabajadores especializados:
        CATALOGO (busca productos), POLITICAS (busca en las políticas de la tienda),
        PEDIDOS (consulta el estado y los productos de un pedido del cliente, si dio un número).
        Responde SOLO con JSON: {"tareas": [{"especialidad": "CATALOGO"|"POLITICAS"|"PEDIDOS", "subtarea": "texto para ese trabajador"}]}
        Incluye solo los trabajadores que realmente se necesiten (mínimo 1, máximo 3).
        """;

    public async Task<RespuestaOrquestada> AtenderAsync(int clienteId, string pregunta, CancellationToken ct)
    {
        // 1 · el ORQUESTADOR (LLM) decide qué trabajadores activar y con qué subtarea
        var plan = await JsonLlm.PedirAsync<PlanOrquestacion>(chat, SistemaOrquestador, pregunta, ct)
            ?? new PlanOrquestacion([new TareaTrabajador("POLITICAS", pregunta)]);

        // 2 · los TRABAJADORES se ejecutan en paralelo (Task.WhenAll), cada uno con su subtarea
        var trabajos = plan.Tareas.Take(3).Select(t => EjecutarTrabajadorAsync(t, clienteId, ct));
        var resultados = await Task.WhenAll(trabajos);

        // 3 · el orquestador SINTETIZA las respuestas de los trabajadores en una sola respuesta
        var contexto = string.Join("\n\n", resultados.Select(r => $"[{r.Especialidad}]\n{r.Respuesta}"));
        List<ChatMessage> sintesis =
        [
            new(ChatRole.System,
                "Sintetiza en español, en máximo 5 líneas, una única respuesta para el cliente " +
                "a partir de las respuestas de los trabajadores. No repitas las etiquetas [CATALOGO], etc."),
            new(ChatRole.User, $"Pregunta original: {pregunta}\n\nRespuestas de los trabajadores:\n{contexto}")
        ];
        var respuestaFinal = await chat.GetResponseAsync(sintesis, cancellationToken: ct);

        return new RespuestaOrquestada(respuestaFinal.Text, resultados);
    }

    private async Task<RespuestaTrabajador> EjecutarTrabajadorAsync(TareaTrabajador tarea, int clienteId, CancellationToken ct)
    {
        var texto = tarea.Especialidad switch
        {
            "CATALOGO" => await TrabajadorCatalogoAsync(tarea.Subtarea, ct),
            "PEDIDOS" => await TrabajadorPedidosAsync(clienteId, ct),
            _ => await TrabajadorPoliticasAsync(tarea.Subtarea, ct)
        };
        return new RespuestaTrabajador(tarea.Especialidad, texto);
    }

    private async Task<string> TrabajadorCatalogoAsync(string subtarea, CancellationToken ct)
    {
        var productos = await tienda.BuscarProductosAsync(subtarea, 5);
        return productos.Count == 0
            ? "No se encontraron productos relacionados."
            : string.Join("; ", productos.Select(p => $"{p.Nombre} (S/ {p.Precio}, stock {p.Stock})"));
    }

    private async Task<string> TrabajadorPoliticasAsync(string subtarea, CancellationToken ct)
    {
        var fragmentos = await buscador.BuscarAsync(subtarea, top: 2, similitudMinima: 0.35, ct: ct);
        return fragmentos.Count == 0
            ? "No se encontró información en las políticas de la tienda."
            : string.Join("\n", fragmentos.Select(f => f.Contenido));
    }

    private async Task<string> TrabajadorPedidosAsync(int clienteId, CancellationToken ct)
    {
        var pedidos = await tienda.PedidosDeClienteAsync(clienteId);
        return pedidos.Count == 0
            ? "El cliente no tiene pedidos registrados."
            : string.Join("; ", pedidos.Select(p => $"Pedido {p.Id}: {p.Estado}"));
    }

    private sealed record TareaTrabajador(string Especialidad, string Subtarea);
    private sealed record PlanOrquestacion(List<TareaTrabajador> Tareas);
}
