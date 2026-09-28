# H4 · RAG (Retrieval-Augmented Generation)

**Concepto:** recuperar → agregar contexto → generar. Se combina con datos transaccionales.

## Qué se agrega
- `RagService`: busca los 4 fragmentos más parecidos (umbral de similitud 0,35),
  los numera, agrega (opcionalmente) los pedidos del cliente y pide al modelo citar `[n]`.
- La respuesta devuelve las **fuentes** usadas.

## Observa
- Compara "¿Cuántos días tengo para devolver?" en H1 (respuesta genérica) y en H4 (30 días, citado).
- Con `clienteId: 1` el modelo combina **política** (30 días) + **dato** (entregado hace 10 días).
- El código decide la secuencia: el modelo no elige si buscar ni dónde.

> "RAG amplía lo que la IA puede consultar o saber, pero no le concede automáticamente
> capacidad para actuar ni decidir el proceso."

## Preguntas
- ¿Qué pasa si subes el umbral de similitud a 0,7?
- ¿Por qué RAG no es un agente? ¿Cómo podría usarlo un agente?
