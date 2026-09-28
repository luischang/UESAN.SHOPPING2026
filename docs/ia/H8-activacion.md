# H8 · Activación (cola + worker programado)

**Concepto:** hasta H7, un humano llama al endpoint del agente. Aquí el agente se activa
solo, a partir de una cola de eventos.

## Qué se agrega
- `ai.EventoAgente`: cola simple (una tabla) con `READPAST/UPDLOCK/ROWLOCK` para que
  varios workers no tomen el mismo evento.
- `AgenteWorker` (`BackgroundService` + `PeriodicTimer`, cada 15 s): revisa la cola y,
  por cada evento pendiente, llama a `AgenteDevoluciones.IniciarAsync` de H7 **sin cambios**.

## Observa
Un "cron" (revisión programada) es solo **otro disparador** del mismo agente: no es un
concepto de IA distinto. Lo que activa al agente puede ser un clic humano (H7), una cola
de eventos (H8) o, más adelante, un servidor MCP (H11).

## Probar
1. `POST /h8/eventos` con un mensaje de devolución.
2. Espera unos 15 segundos y consulta `GET /h8/eventos`: verás `EjecucionId` lleno.
3. `GET /h7/agente/{EjecucionId}/pasos` para ver la traza, igual que en H7.

## Preguntas
- ¿Qué otros eventos de un e-commerce real podrían activar a este agente?
- ¿Qué pasaría si dos instancias de la API corrieran al mismo tiempo? (Pista: `READPAST`.)
