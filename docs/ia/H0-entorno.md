# H0 · Entorno del laboratorio

**Concepto:** antes de hablar de IA, necesitamos datos transaccionales reales y un modelo disponible.

## Qué se agrega
- `docker-compose.yml`: SQL Server 2022, Ollama y la descarga de modelos.
- `docker/sql`: base `UesanShoppingAI` con categorías, productos, clientes y pedidos.
- `UESAN.SHOPPING.AI`: biblioteca de IA (por ahora solo el repositorio Dapper).
- `UESAN.SHOPPING.AI.API`: Minimal API con Swagger.

## Probar
1. `docker compose up -d` y esperar a que `sql-init` y `ollama-init` terminen (`docker compose ps -a`).
2. Ejecutar la API y abrir `GET /api/tienda/pedidos/1001`.

## Preguntas
- ¿Qué datos de la tienda son transaccionales y cuáles serían conocimiento documental?
- ¿Por qué la IA vive en un proyecto separado de la API principal?
