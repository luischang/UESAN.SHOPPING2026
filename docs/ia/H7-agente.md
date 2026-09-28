# H7 · Agente

**Concepto:** el LLM dirige su propio proceso — observa, decide qué herramienta usar,
actúa y evalúa el resultado — dentro de límites que el código controla.

## Ciclo
```
mientras no se alcance un límite:
    observar (historial) → decidir (el modelo elige una herramienta o responde)
    actuar   (se ejecuta la herramienta, siempre a través de la Puerta de H5)
    evaluar  (el resultado se agrega al historial; el modelo lo ve en la próxima vuelta)
```

## Qué se agrega
- `AgenteOptions`: `MaxPasos`, `PresupuestoTokens`, `TiempoMaximoSegundos`, `UmbralAprobacionReembolso`.
  **Autonomía no es carta blanca**: estos límites los fija el código, no el modelo.
- `AgenteDevoluciones.EjecutarAsync`: el ciclo. Usa `FunctionCallContent` /
  `FunctionResultContent` "en crudo" (sin `UseFunctionInvocation`) porque el agente necesita
  **interceptar** ciertas herramientas antes de ejecutarlas.
- Dos puntos de **suspensión**: `SolicitarEvidencia` (espera al cliente) y un reembolso
  mayor al umbral (espera a un supervisor). El estado se guarda en `ai.EjecucionAgente`
  (`HistorialJson`) y se puede **reanudar** más tarde.
- `EscalarAHumano`: termina la ejecución y la deja para una persona de postventa.
- Toda herramienta sigue pasando por la `PuertaHerramientas` de H5: el agente no se salta
  la autorización, las reglas de negocio ni la auditoría.

## Probar
1. Caso simple (mouse, pedido 1001): normalmente se resuelve en 2-3 pasos.
2. `GET /h7/agente/{id}/pasos`: compara con la traza de H6 — aquí el modelo eligió el orden.
3. Caso con monto alto (monitor, pedido 1004): queda `ESPERANDO_APROBACION`.
4. `GET /h7/aprobaciones` y `POST /h7/aprobaciones/{id}` para aprobar y que el agente continúe.

## Preguntas
- ¿Qué pasaría si `MaxPasos` fuera 2 en el caso del monitor?
- ¿Por qué `EmitirReembolso` sigue validando el monto aunque ya haya aprobación?
- Compara con H6: ¿qué ganamos con un agente? ¿qué perdemos en previsibilidad?
