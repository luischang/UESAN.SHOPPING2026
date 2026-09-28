# H5 · Herramientas (function / tool calling)

**Concepto:** el modelo **solicita** una herramienta; el **código** decide si se ejecuta.

## Qué se agrega
- `HerramientasTienda`: `ConsultarProducto`, `ConsultarPedido`, `BuscarPolitica` (informar) y
  `RegistrarDevolucion` (actuar).
- `PuertaHerramientas`: allowlist, validación, autorización con el cliente real, ejecución,
  resultado en JSON y auditoría en `ai.Auditoria`.
- `ReglasDevolucion`: la política hecha código (plazo, estado, producto retornable).
- Idempotencia: restricción única `(PedidoId, ProductoId)`.
- `FunctionInvokingChatClient` (Microsoft.Extensions.AI) invoca las funciones automáticamente.

## Probar
Usa la cabecera `X-Cliente-Id` (solo para laboratorio; en H9 se reemplaza por JWT).
1. Consultar estado del pedido 1002 (cliente 1).
2. Devolver audífonos del pedido 1001 (cliente 1). Repite: la segunda vez es idempotente.
3. Cliente 1 pregunta por el pedido 1004 (de Bruno): la puerta lo rechaza.
4. Cliente 2, pedido 1003: fuera de plazo.
5. `GET /h5/auditoria`.

## Preguntas
- ¿Dónde está la regla de los 30 días: en el prompt o en el código? ¿Por qué importa?
- ¿Esto ya es un agente? (Pista: ¿quién define el objetivo y el recorrido?)
