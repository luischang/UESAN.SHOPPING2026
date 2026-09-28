# H10 · Multiagente (orquestador-trabajadores)

**Concepto:** un LLM orquestador descompone la pregunta en subtareas y las reparte entre
varios "trabajadores" que se ejecutan en paralelo; luego el orquestador sintetiza.

## Nota sobre la fuente (verificada)
Anthropic, en *Building effective agents*, describe este patrón como **orchestrator-workers**
y lo clasifica como un **WORKFLOW**, no como un tipo aparte de "agente". La diferencia frente
a H6 es que aquí el reparto de subtareas **no está predefinido**: lo decide el orquestador en
cada ejecución, según la pregunta.

> "Agentic AI" usado como sinónimo de "sistema multiagente" es una simplificación
> **didáctica** de este curso, para ordenar la progresión de temas — **no es una
> equivalencia que use Anthropic**.

## Qué se agrega
- `OrquestadorPostventaService`: el LLM devuelve un plan JSON con hasta 3 tareas
  (`CATALOGO`, `POLITICAS`, `PEDIDOS`); se ejecutan con `Task.WhenAll` y se sintetizan.

## Probar
- Pregunta compuesta → revisa `trabajadores`: verás las 3 especialidades trabajando en paralelo.
- Pregunta simple → normalmente solo activa `POLITICAS`.

## Preguntas
- ¿En qué se parece este patrón a la ruta backend developer con microservicios?
- ¿Qué pasa si dos trabajadores fallan pero uno responde bien? ¿Qué debería hacer la síntesis?
