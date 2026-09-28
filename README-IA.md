# UESAN Shopping · Rama de IA (semanas 7 y 8)

Esta rama muestra, commit a commit, cómo evoluciona una aplicación web con IA:
desde una pregunta a un LLM hasta un sistema con agentes, herramientas y MCP.
Cada commit es un **hito** (`H0` … `H11`) y tiene una etiqueta `ia-h0` … `ia-h11`
para poder volver a él con `git checkout ia-hN`.

> La API principal (`UESAN.SHOPPING.API`) no se modifica. Toda la IA vive en
> `UESAN.SHOPPING.AI` (lógica) y `UESAN.SHOPPING.AI.API` (Minimal API).

## Hitos

| Hito | Semana | Concepto | Qué se agrega |
|---|---|---|---|
| H0 | 7 | Entorno | Docker (SQL Server + Ollama), base de datos de la tienda, proyectos de IA |
| H1 | 7 | Pregunta y respuesta con un LLM | `IChatClient` (Microsoft.Extensions.AI) con Ollama local u OpenAI en la nube |
| H2 | 7 | Datos transaccionales como contexto | El código consulta los pedidos del cliente y los agrega al prompt |
| H3 | 7 | Embeddings y búsqueda vectorial | Ingesta de políticas, vectores en SQL Server, similitud coseno |
| H4 | 8 | RAG | Recuperar → agregar contexto → generar, con fuentes citadas |
| H5 | 8 | Herramientas (tool calling) | El modelo solicita; la **puerta de herramientas** valida, autoriza, ejecuta y audita |
| H6 | 8 | Workflow con IA | Camino fijo en código; el LLM clasifica, extrae y redacta |
| H7 | 8 | Agente | Ciclo observar → decidir → actuar → evaluar, con límites y aprobación humana |
| H8 | 8 | Activación | Cola de eventos + `BackgroundService` y revisión programada |
| H9 | 8 | Seguridad y gobierno | JWT, CORS, rate limiting, prompt injection, presupuesto |
| H10 | 8 | Multiagente | Orquestador-trabajadores (patrón descrito por Anthropic) |
| H11 | 8 | MCP | Servidor MCP con herramientas de solo lectura (`ConsultarProducto`, `BuscarPolitica`) |

Cada hito tiene su guía en `docs/ia/`.

## Cómo ejecutar

Requisitos: Docker Desktop, .NET 10 SDK, Visual Studio 2026 (o VS Code).

```bash
docker compose up -d          # SQL Server (14330) + Ollama (11434) + modelos + datos
```

La primera vez descarga los modelos (`qwen2.5:3b` y `bge-m3`, ~3 GB). Luego:

- Visual Studio: abrir `UESAN.SHOPPING.slnx`, proyecto de inicio `UESAN.SHOPPING.AI.API`, perfil `http`.
- Consola: `dotnet run --project UESAN.SHOPPING.AI.API`
- Todo en contenedores: `docker compose --profile api up -d --build`

Swagger: http://localhost:5080/swagger · Pruebas: `UESAN.SHOPPING.AI.API/UESAN.SHOPPING.AI.API.http`

Al cambiar de hito (`git checkout ia-hN`) vuelve a correr `docker compose up -d`:
los scripts SQL son idempotentes y agregan las tablas que el hito necesita.

## Datos de prueba

| Cliente | Pedido | Situación |
|---|---|---|
| 1 · Ana | 1001 | Entregado hace 10 días (audífonos + mouse): devolución posible |
| 1 · Ana | 1002 | Enviado, aún no entregado |
| 2 · Bruno | 1003 | Entregado hace 45 días: fuera de plazo |
| 2 · Bruno | 1004 | Monitor de S/ 1299: reembolso requiere aprobación humana |
| 3 · Carla | 1005 | Cepillo eléctrico (higiene) y licencia digital: no retornables |
| 3 · Carla | 1006 | Pendiente |
