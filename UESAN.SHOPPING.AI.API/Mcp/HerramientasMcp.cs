using System.ComponentModel;
using ModelContextProtocol.Server;
using UESAN.SHOPPING.AI.Datos;
using UESAN.SHOPPING.AI.H3_Embeddings;

namespace UESAN.SHOPPING.AI.API.Mcp;

/// <summary>
/// H11 · Servidor MCP: expone la tienda como HERRAMIENTAS que CUALQUIER cliente MCP puede
/// usar (Claude Desktop, VS Code, otro agente...), no solo esta API.
///
/// MCP (Model Context Protocol) lo introdujo Anthropic en noviembre de 2024 como un
/// estándar abierto; el 9 de diciembre de 2025 Anthropic lo donó a la Agentic AI
/// Foundation (un fondo de la Linux Foundation) para que su gobierno sea neutral y
/// comunitario, junto a Block (goose) y OpenAI (AGENTS.md) como proyectos fundadores.
///
/// Por seguridad de laboratorio, este servidor SOLO expone herramientas de LECTURA
/// (no registra devoluciones ni emite reembolsos): un cliente MCP externo aún no pasa
/// por la autenticación de cliente de H9, así que no debe poder actuar sobre la tienda.
/// </summary>
[McpServerToolType]
public static class HerramientasMcp
{
    [McpServerTool, Description("Busca productos del catálogo de UESAN Shopping por nombre, descripción o categoría.")]
    public static async Task<string> ConsultarProducto(
        TiendaRepositorio tienda,
        [Description("Texto a buscar, por ejemplo 'audífonos' o 'libros'")] string texto)
    {
        var productos = await tienda.BuscarProductosAsync(texto, 10);
        return productos.Count == 0
            ? "No se encontraron productos."
            : string.Join("\n", productos.Select(p => $"- {p.Nombre}: S/ {p.Precio}, stock {p.Stock}"));
    }

    [McpServerTool, Description("Busca en las políticas de UESAN Shopping (devoluciones, envíos, garantías, medios de pago).")]
    public static async Task<string> BuscarPolitica(
        BuscadorSemantico buscador,
        [Description("Consulta en lenguaje natural, por ejemplo 'plazo para devolver un producto'")] string consulta)
    {
        var fragmentos = await buscador.BuscarAsync(consulta, top: 3, similitudMinima: 0.35);
        return fragmentos.Count == 0
            ? "No se encontró información relacionada en las políticas."
            : string.Join("\n\n", fragmentos.Select(f => $"({f.Fuente} › {f.Seccion})\n{f.Contenido}"));
    }
}
