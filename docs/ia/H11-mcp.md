# H11 · Model Context Protocol (MCP)

**Concepto:** en vez de escribir un cliente distinto para cada herramienta, MCP es un
estándar abierto: cualquier cliente compatible (Claude Desktop, VS Code, otro agente)
puede descubrir y usar las herramientas que expone un servidor MCP.

## Contexto verificado
Anthropic introdujo MCP en noviembre de 2024. El 9 de diciembre de 2025 lo **donó a la
Agentic AI Foundation**, un fondo dirigido bajo la Linux Foundation, cofundado por
Anthropic, Block y OpenAI (con el apoyo de Google, Microsoft, AWS, Cloudflare y
Bloomberg), junto a *goose* (Block) y *AGENTS.md* (OpenAI) como proyectos fundadores.
El objetivo declarado es que el protocolo evolucione de forma neutral y comunitaria.

## Qué se agrega
- `ModelContextProtocol.AspNetCore`: `AddMcpServer().WithHttpTransport().WithToolsFromAssembly()`
  y `MapMcp("/mcp")`.
- `HerramientasMcp`: **solo lectura** (`ConsultarProducto`, `BuscarPolitica`). Por diseño,
  este servidor NO expone `RegistrarDevolucion` ni `EmitirReembolso`: un cliente MCP externo
  todavía no pasa por la autenticación de cliente de H9, así que no debe poder actuar.

## Probar
- Con un cliente MCP (por ejemplo, el inspector de MCP o Claude Desktop configurado con
  la URL `http://localhost:5080/mcp`), listar las herramientas disponibles y llamarlas.

## Preguntas
- ¿Por qué H11 solo expone herramientas de lectura, a diferencia de H5-H7?
- Compara MCP con lo que hiciste en H1-H4: ¿qué problema nuevo resuelve MCP frente a
  simplemente llamar a `IChatClient` directamente?
- ¿Qué tendría que cambiar en H11 para poder exponer `RegistrarDevolucion` de forma segura?
