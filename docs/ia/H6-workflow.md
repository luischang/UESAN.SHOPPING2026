# H6 · Workflow con IA

**Concepto:** el camino está escrito en el código; el LLM trabaja dentro de pasos fijos.

```
Clasificar (LLM) → Validar pedido (código) → Extraer datos (LLM) → Resolver producto (código)
→ Validar política (código) → Registrar (código) → Redactar notificación (LLM)
```

## Qué se agrega
- `DevolucionWorkflow` con la traza `pasos` (número, paso, responsable, resultado).
- `JsonLlm`: pide JSON al modelo y lo interpreta de forma tolerante.

## Relación con Anthropic
En "Building effective agents", Anthropic llama a esto **prompt chaining**: una secuencia de
llamadas al LLM con verificaciones programáticas (*gates*) entre pasos. Es un **workflow**,
no un agente: las rutas están predefinidas en el código.

## Preguntas
- Si mañana hay que agregar "verificar stock para cambio", ¿qué se modifica?
- ¿Qué ventajas tiene este workflow frente a un agente para este caso?
